#!/usr/bin/env python3
"""
Migrate historical expense data from the user's real Excel bookkeeping file
("สมุดบัญชีรายวัน ปี 67-69") into the app.

The source file stores one MONTHLY TOTAL per (month, category) cell — not one
row per transaction. The real per-day breakdown is hidden in each cell's
Excel comment, e.g.:

    31/3=500
    6/4=100 ก้วยเตียว
    13/4 ข้าวเช้า =168
    หม้อหุ้งข้าว 639          <- no date at all

This script reads the JSON produced by extracting that grid (see
`expense_grid.json` — one entry per non-empty cell: month, year, columnPath,
cellTotal, comment), parses each comment line into (date-or-none, desc,
amount-or-none), and turns it into real transactions via the API.

Owner mapping (see the conversation this was designed in): columns that name
a specific person use that person's account; every column left unnamed in
the plan defaults to ป๊า (พ่อ).

Modes:
  Dry-run (default): parse everything, print a report, write nothing.
  --commit:          actually create categories + transactions via the API.

Usage:
    python scripts/migrate_excel_data.py --grid path/to/expense_grid.json
    python scripts/migrate_excel_data.py --grid path/to/expense_grid.json --commit
"""
import argparse
import json
import re
import urllib.error
import urllib.request
from collections import defaultdict

API = "http://localhost:5001/api"
ADMIN_EMAIL = "pongpan22120@gmail.com"

# Explicit per-column owner mapping. Anything not listed here falls back to
# DEFAULT_OWNER (ป๊า) — this was decided column-by-column with the user rather
# than guessed by substring match, to avoid traps like "แม่บ้าน" containing "แม่".
OWNER_MAP = {
    "สมาชิกในบ้าน > ป๊า": "paa@family.local",
    "สมาชิกในบ้าน > แม่": "mae@family.local",
    "สมาชิกในบ้าน > เนย": "noei@family.local",
    "สมาชิกในบ้าน > โบ๊ต": ADMIN_EMAIL,
    "ค่าใช้จ่ายอาหาร > แม่จ่าย": "mae@family.local",
    "ค่าใช้จ่ายอาหาร > ป๊าจ่าย": "paa@family.local",
    "ค่าโทรศัพท์ > พ่อ": "paa@family.local",
    "ค่าโทรศัพท์ > แม่": "mae@family.local",
}
DEFAULT_OWNER = "paa@family.local"

# Income has no explicit person names in any column at all — everything
# defaults to ป๊า (confirmed with the user).
INCOME_OWNER_MAP = {}

# Lines in the "รายรับ" sheet's comments that literally describe borrowed
# money (เงินยืม / ยืมเงิน... / เงินกู้) are still real cash coming in, but
# they're debt, not earned income — route them to their own category instead
# of blending them into whatever column they were jotted under.
LOAN_KEYWORDS_RE = re.compile(r"ยืม|กู้")
LOAN_CATEGORY_NAME = "เงินกู้ยืม"
LOAN_GIVEN_CATEGORY_NAME = "ให้ยืมเงิน"

# When a loan line names a family member as the lender (e.g. "ยืมเงินแม่"),
# it's a transfer between household accounts, not free money — mirror it as
# an expense on the lender's own account so their balance reflects it too.
# Longest names first so "ป๊า" doesn't shadow a longer match, checked in order.
FAMILY_NAME_TO_EMAIL = [
    ("โบ๊ต", ADMIN_EMAIL),
    ("เนย", "noei@family.local"),
    ("แม่", "mae@family.local"),
    ("ป๊า", "paa@family.local"),
    ("พ่อ", "paa@family.local"),
]


def lender_for_loan_line(desc, receiver_owner):
    for name, email in FAMILY_NAME_TO_EMAIL:
        if name in (desc or "") and email != receiver_owner:
            return email
    return None

PASSWORD_BY_EMAIL = {
    "paa@family.local": "Family@12345",
    "mae@family.local": "Family@12345",
    "noei@family.local": "Family@12345",
    ADMIN_EMAIL: "Admin@123456",
}

THAI_MONTHS = {
    "มกราคม": 1, "กุมภาพันธ์": 2, "มีนาคม": 3, "เมษายน": 4,
    "พฤษภาคม": 5, "มิถุนายน": 6, "กรกฎาคม": 7, "สิงหาคม": 8,
    "กันยายน": 9, "ตุลาคม": 10, "พฤศจิกายน": 11, "ธันวาคม": 12,
}

DATE_LINE_RE = re.compile(r"^\s*(\d{1,2})\s*/\s*(\d{1,2})\s*[=]?\s*(.*)$")
NUMBER_RE = re.compile(r"\d[\d,]*\.?\d*")
AUTHOR_LINE_RE = re.compile(r"^[A-Za-z]+:\s*$")

DEFAULT_CATEGORY_ICON = "\U0001F4C1"  # 📁
DEFAULT_CATEGORY_COLOR = "#607D8B"


# ---------- HTTP helpers (same pattern as seed_test_data.py) ----------

def call(method, path, body=None, token=None):
    data = json.dumps(body).encode("utf-8") if body is not None else None
    req = urllib.request.Request(f"{API}{path}", data=data, method=method)
    req.add_header("Content-Type", "application/json; charset=utf-8")
    if token:
        req.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(req) as r:
            return r.status, json.loads(r.read().decode("utf-8") or "null")
    except urllib.error.HTTPError as e:
        text = e.read().decode("utf-8")
        try:
            return e.code, json.loads(text)
        except ValueError:
            return e.code, text


def value(result):
    return result.get("value", result) if isinstance(result, dict) else result


def login(email):
    password = PASSWORD_BY_EMAIL[email]
    status, body = call("POST", "/Authorize/login", {"email": email, "password": password})
    if status != 200:
        raise RuntimeError(f"Login failed for {email}: {status} {body}")
    data = value(body)
    return data["accessToken"] if "accessToken" in data else data["token"]


# ---------- comment parsing ----------

def parse_amount(text):
    """Pull the trailing amount-looking number out of a text fragment."""
    matches = list(NUMBER_RE.finditer(text))
    if not matches:
        return None, text.strip()
    m = matches[-1]
    amount = float(m.group(0).replace(",", ""))
    desc = (text[:m.start()] + text[m.end():]).strip(" =\t-")
    return amount, desc


def parse_comment(comment):
    """Return a list of {day, month, desc, amount, inferred_date}."""
    items = []
    current_date = None  # (day, month)
    for raw_line in comment.split("\n"):
        line = raw_line.strip()
        if not line or AUTHOR_LINE_RE.match(line):
            continue

        is_continuation = raw_line != raw_line.lstrip()
        m = DATE_LINE_RE.match(line)
        if m:
            day, mon, rest = int(m.group(1)), int(m.group(2)), m.group(3)
            current_date = (day, mon)
            amount, desc = parse_amount(rest)
            items.append({"day": day, "month": mon, "desc": desc or line, "amount": amount, "inferred_date": False})
        elif is_continuation and current_date:
            amount, desc = parse_amount(line)
            items.append({"day": current_date[0], "month": current_date[1], "desc": desc or line, "amount": amount, "inferred_date": False})
        else:
            amount, desc = parse_amount(line)
            items.append({"day": None, "month": None, "desc": desc or line, "amount": amount, "inferred_date": True})
    return items


def to_gregorian(buddhist_year):
    return buddhist_year - 543


def resolve_transaction_date(entry, item):
    """Turn (row month/year, parsed item) into a concrete YYYY-MM-DD string + whether it was inferred."""
    row_month_num = THAI_MONTHS[entry["month"]]
    year = to_gregorian(entry["year"])

    if item["day"] is not None and not item["inferred_date"]:
        month_num = item["month"]
        # A dated line can reference the previous month's last day (seen in
        # the real file, e.g. "31/3" inside the เมษายน cell).
        if month_num != row_month_num:
            if row_month_num == 1 and month_num == 12:
                year -= 1
            # otherwise assume same year, just previous/adjacent month
        try:
            return f"{year:04d}-{month_num:02d}-{item['day']:02d}", False
        except ValueError:
            pass  # invalid day for that month, fall through to fallback

    return f"{year:04d}-{row_month_num:02d}-01", True


def owner_for(column_path, owner_map):
    return owner_map.get(column_path, DEFAULT_OWNER)


def category_name_for(column_path):
    return column_path.replace(" > ", " - ")


# ---------- core processing ----------

def whole_cell_total_txn(owner, category, entry, month_name, note, txn_type):
    row_month_num = THAI_MONTHS[month_name]
    return {
        "owner": owner,
        "category": category,
        "amount": entry["cellTotal"],
        "date": f"{to_gregorian(entry['year']):04d}-{row_month_num:02d}-01",
        "note": note,
        "type": txn_type,
    }


def build_transactions(grid, owner_map, is_income=False):
    """Returns (transactions, flagged, stats) — transactions is a list of dicts
    ready to POST, flagged is a list of human-readable strings for cells that
    had neither a usable comment breakdown nor a cell total to fall back to.

    Per-line dates/amounts from the comment are only used when EVERY line in
    that cell parsed to a valid amount AND their sum reconciles with the
    cell's own displayed total. Otherwise the whole cell falls back to one
    lump-sum transaction using the cell's own total directly — this is old
    data, so a clean total beats an uncertain per-line reconstruction.

    When is_income is set, any line whose description mentions borrowing
    money (ยืม/กู้) is routed to LOAN_CATEGORY_NAME instead of the column's
    own category — it's still cash in, but debt, not earned income.
    """
    transactions = []
    flagged = []
    stats = {"dated": 0, "inferred": 0, "no_comment_total": 0, "fallback_whole_cell": 0, "loan_pairs": 0}
    sheet_type = "Income" if is_income else "Expense"

    for entry in grid:
        column_path = entry["columnPath"]
        owner = owner_for(column_path, owner_map)
        category = category_name_for(column_path)
        month_name = entry["month"]
        comment = entry["comment"] or ""
        cell_total = entry["cellTotal"]
        has_total = cell_total is not None and cell_total > 0

        if not comment.strip():
            # No itemized breakdown at all — just the raw monthly total.
            if has_total:
                transactions.append(whole_cell_total_txn(
                    owner, category, entry, month_name,
                    f"ยอดรวมเดือน{month_name} (ไม่มีรายละเอียดย่อยในไฟล์ต้นฉบับ)",
                    sheet_type,
                ))
                stats["no_comment_total"] += 1
            continue

        items = parse_comment(comment)
        resolved = []
        parsed_sum = 0.0
        all_valid = True
        for item in items:
            if item["amount"] is None or item["amount"] <= 0:
                all_valid = False
                continue
            date_str, inferred = resolve_transaction_date(entry, item)
            resolved.append((item, date_str, inferred))
            parsed_sum += item["amount"]

        reconciles = cell_total is None or abs(cell_total - parsed_sum) <= 1.0

        if all_valid and reconciles:
            for item, date_str, inferred in resolved:
                note = item["desc"] or column_path
                row_category = category
                is_loan = is_income and LOAN_KEYWORDS_RE.search(item["desc"] or "")
                if is_loan:
                    row_category = LOAN_CATEGORY_NAME
                if inferred:
                    note = f"{note} {item['amount']:g} — ไม่พบวันที่ระบุใน comment ต้นฉบับ ใช้วันที่ 1 {month_name} แทน"
                    stats["inferred"] += 1
                else:
                    stats["dated"] += 1
                transactions.append({
                    "owner": owner,
                    "category": row_category,
                    "amount": item["amount"],
                    "date": date_str,
                    "note": note,
                    "type": sheet_type,
                })

                if is_loan:
                    lender = lender_for_loan_line(item["desc"], owner)
                    if lender:
                        transactions.append({
                            "owner": lender,
                            "category": LOAN_GIVEN_CATEGORY_NAME,
                            "amount": item["amount"],
                            "date": date_str,
                            "note": f"ให้ {OWNER_DISPLAY_NAME.get(owner, owner)} ยืม — {item['desc']}",
                            "type": "Expense",
                        })
                        stats["loan_pairs"] += 1
        elif has_total:
            # Comment breakdown was incomplete/inconsistent — old data, so
            # just trust the cell's own total instead of the comment. Still
            # worth checking the raw comment text for a loan mention here —
            # a whole cell can fail reconciliation (partial notes, running
            # daily entries that don't quite add up) while still clearly
            # being "borrowed from X" per its own text.
            fallback_category = category
            is_loan = is_income and LOAN_KEYWORDS_RE.search(comment)
            if is_loan:
                fallback_category = LOAN_CATEGORY_NAME
            fallback_note = f"ยอดรวมเดือน{month_name} (ใช้ยอดรวมจากไฟล์ต้นฉบับ แยกรายละเอียดจาก comment ไม่ได้ชัดเจน)"
            transactions.append(whole_cell_total_txn(
                owner, fallback_category, entry, month_name, fallback_note, sheet_type,
            ))
            stats["fallback_whole_cell"] += 1

            if is_loan:
                lender = lender_for_loan_line(comment, owner)
                if lender:
                    transactions.append({
                        "owner": lender,
                        "category": LOAN_GIVEN_CATEGORY_NAME,
                        "amount": cell_total,
                        "date": f"{to_gregorian(entry['year']):04d}-{THAI_MONTHS[month_name]:02d}-01",
                        "note": f"ให้ {OWNER_DISPLAY_NAME.get(owner, owner)} ยืม — {fallback_note}",
                        "type": "Expense",
                    })
                    stats["loan_pairs"] += 1
        else:
            flagged.append(
                f"[{month_name} {entry['year']} / {column_path}] แยกรายละเอียดจาก comment ไม่ได้ และไม่มียอดรวมในเซลล์ให้ใช้แทน: "
                f"\"{comment[:80]}\""
            )

    return transactions, flagged, stats


def print_report(transactions, flagged, stats):
    total_lines = stats["dated"] + stats["inferred"]
    print("=== Dry-run report ===")
    print(f"Transactions that would be created: {len(transactions)}")
    print(f"  - with a real parsed date       : {stats['dated']}")
    print(f"  - fallback to day 1 (no date)   : {stats['inferred']}")
    print(f"  - whole-cell totals (no comment): {stats['no_comment_total']}")
    print(f"  - whole-cell totals (fallback)  : {stats['fallback_whole_cell']}")
    print(f"  - loan lines paired with lender : {stats['loan_pairs']}")
    if total_lines:
        pct = 100 * stats["dated"] / total_lines
        print(f"  => {pct:.1f}% of itemized lines had a real date")
    print(f"\nFlagged (no amount found, skipped): {len(flagged)}")
    for f in flagged[:30]:
        print(f"  - {f}")
    if len(flagged) > 30:
        print(f"  ... and {len(flagged) - 30} more")

    by_owner_type = defaultdict(float)
    for t in transactions:
        by_owner_type[(t["owner"], t["type"])] += t["amount"]
    print("\nTotal by owner:")
    for (owner, ttype), total in sorted(by_owner_type.items()):
        print(f"  {owner} [{ttype}]: {total:,.2f}")


OWNER_DISPLAY_NAME = {
    "paa@family.local": "ป๊า",
    "mae@family.local": "แม่",
    "noei@family.local": "เนย",
    ADMIN_EMAIL: "โบ๊ต",
}


def create_categories(transactions):
    """Create any missing categories via the API for each owner — required
    before the web Import wizard can match rows to a category. Creates only
    categories, never transactions."""
    tokens = {}
    needed = defaultdict(dict)  # owner -> {category name: type}
    for t in transactions:
        needed[t["owner"]][t["category"]] = t["type"]

    for owner, name_types in needed.items():
        tokens[owner] = login(owner)
        print(f"logged in as {owner}")
        status, body = call("GET", "/Categories", token=tokens[owner])
        existing = {c["name"] for c in value(body)} if status == 200 else set()

        for name in sorted(name_types):
            if name in existing:
                continue
            status, body = call(
                "POST", "/Categories",
                {"name": name, "type": name_types[name], "icon": DEFAULT_CATEGORY_ICON, "color": DEFAULT_CATEGORY_COLOR},
                tokens[owner],
            )
            if status == 200:
                print(f"  created category '{name}' ({name_types[name]}) for {owner}")
            elif status == 409:
                print(f"  '{name}' already exists for {owner}, skipping")
            else:
                print(f"  FAILED creating '{name}' for {owner}: {status} {body}")


def export_xlsx(transactions, flagged, out_dir, txn_type):
    """Write one .xlsx per owner, formatted for the web app's own Import
    wizard (วันที่ / ประเภท / หมวดหมู่ / จำนวนเงิน / หมายเหตุ columns), plus a
    separate file listing everything that was flagged for manual review."""
    from openpyxl import Workbook
    import os

    type_label = "รายรับ" if txn_type == "Income" else "รายจ่าย"
    os.makedirs(out_dir, exist_ok=True)
    by_owner = defaultdict(list)
    for t in transactions:
        by_owner[t["owner"]].append(t)

    for owner, rows in by_owner.items():
        name = OWNER_DISPLAY_NAME.get(owner, owner)
        path = os.path.join(out_dir, f"{name}-{type_label}.xlsx")
        wb = Workbook()
        ws = wb.active
        ws.title = type_label
        ws.append(["วันที่", "ประเภท", "หมวดหมู่", "จำนวนเงิน", "หมายเหตุ"])
        for t in sorted(rows, key=lambda r: r["date"]):
            row_type_label = "รายรับ" if t["type"] == "Income" else "รายจ่าย"
            ws.append([t["date"], row_type_label, t["category"], t["amount"], t["note"]])
        wb.save(path)
        print(f"wrote {len(rows)} rows -> {path}")

    if flagged:
        wb = Workbook()
        ws = wb.active
        ws.title = "ต้องตรวจสอบเอง"
        ws.append(["รายละเอียด"])
        for f in flagged:
            ws.append([f])
        path = os.path.join(out_dir, f"ต้องตรวจสอบเอง-{type_label}.xlsx")
        wb.save(path)
        print(f"wrote {len(flagged)} flagged items -> {path}")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--grid", required=True, help="Path to expense_grid.json or income_grid.json")
    parser.add_argument("--sheet", choices=["expense", "income"], default="expense", help="Which source sheet this grid came from")
    parser.add_argument("--create-categories", action="store_true", help="Create missing categories via the API (metadata only, no transactions)")
    parser.add_argument("--export-dir", help="Directory to write per-owner .xlsx files + the flagged-items file")
    args = parser.parse_args()

    is_income = args.sheet == "income"
    txn_type = "Income" if is_income else "Expense"
    owner_map = INCOME_OWNER_MAP if is_income else OWNER_MAP

    with open(args.grid, encoding="utf-8-sig") as f:
        grid = json.load(f)

    transactions, flagged, stats = build_transactions(grid, owner_map, is_income=is_income)
    print_report(transactions, flagged, stats)

    if args.create_categories:
        print("\n=== Creating categories ===")
        create_categories(transactions)

    if args.export_dir:
        print("\n=== Exporting .xlsx files ===")
        export_xlsx(transactions, flagged, args.export_dir, txn_type)

    if not args.create_categories and not args.export_dir:
        print("\n(dry-run only — re-run with --create-categories and/or --export-dir to produce output)")


if __name__ == "__main__":
    main()

#!/usr/bin/env python3
"""
Seed a family of 4 test accounts with ~1 month of transactions and a few
TransactionProposals (pending/accepted/rejected), for manual UI testing.

pongpan22120@gmail.com is seeded directly as the system Admin by the API on
startup (see SeedAdminUserAsync in SummaryYearSalary.Api/Program.cs) — this
script just logs into it rather than registering it. The other 3 family
accounts self-register as normal "User" role accounts.

Usage:
    1. Reset the DB first if you want a clean slate:
         dotnet ef database drop --force --project SummaryYearSalary.Infrastructure --startup-project SummaryYearSalary.Api
         dotnet ef database update --project SummaryYearSalary.Infrastructure --startup-project SummaryYearSalary.Api
    2. Start the API (http://localhost:5001) so it re-seeds default categories/roles/admin.
    3. Run: python scripts/seed_test_data.py

Safe to re-run: registering an email that already exists just falls through
to logging in with it, so a second run tops up more transactions/proposals
for the same 4 accounts rather than failing.
"""
import json
import random
import urllib.error
import urllib.request
from datetime import datetime, timedelta

API = "http://localhost:5001/api"
PASSWORD = "Test@12345"

ADMIN_EMAIL = "pongpan22120@gmail.com"
ADMIN_PASSWORD = "Admin@123456"

# (email, first name, last name) — a family of 4; user 1 is pongpan's real
# account, seeded as the system Admin (see ADMIN_EMAIL/ADMIN_PASSWORD above).
USERS = [
    (ADMIN_EMAIL, "พงศ์พันธ์", "(ผู้ใช้เอง)"),
    ("testuser2@example.com", "พ่อ", "ทดสอบ"),
    ("testuser3@example.com", "แม่", "ทดสอบ"),
    ("testuser4@example.com", "พี่", "ทดสอบ"),
]

EXPENSE_NOTES = {
    "อาหาร/เครื่องดื่ม": ["ข้าวกลางวัน", "กาแฟ", "ก๋วยเตี๋ยว", "ซื้อกับข้าว", "ของหวาน"],
    "ที่พักอาศัย": ["ค่าเช่าบ้าน", "ค่าส่วนกลาง", "ซ่อมแซมบ้าน"],
    "การเดินทาง": ["ค่าน้ำมัน", "ค่าทางด่วน", "ค่า BTS", "ค่าแท็กซี่"],
    "ช้อปปิ้ง": ["ซื้อเสื้อผ้า", "ของใช้ในบ้าน", "ช้อปออนไลน์"],
    "สุขภาพ": ["ซื้อยา", "ค่าหมอ", "ค่าตรวจสุขภาพ"],
    "บันเทิง": ["ดูหนัง", "สตรีมมิ่ง", "เที่ยวสวนสนุก"],
    "การศึกษา": ["ค่าคอร์สเรียน", "ซื้อหนังสือ"],
    "ประกัน": ["เบี้ยประกันสุขภาพ", "เบี้ยประกันรถ"],
    "สาธารณูปโภค": ["ค่าไฟ", "ค่าน้ำ", "ค่าอินเทอร์เน็ต", "ค่ามือถือ"],
    "อื่นๆ (รายจ่าย)": ["เบ็ดเตล็ด", "ค่าใช้จ่ายอื่นๆ"],
}
INCOME_NOTES = {
    "เงินเดือน": ["เงินเดือนประจำเดือน"],
    "โบนัส": ["โบนัสประจำปี"],
    "รายได้เสริม": ["งานฟรีแลนซ์", "ขายของออนไลน์"],
    "ดอกเบี้ย/ปันผล": ["ดอกเบี้ยเงินฝาก"],
}


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


def register_and_login(email, first_name, last_name, password=PASSWORD):
    # For pongpan (ADMIN_EMAIL) this register call is expected to fail — the
    # account already exists, seeded directly as Admin by the API on startup.
    call(
        "POST",
        "/Authorize/register",
        {
            "firstName": first_name,
            "lastName": last_name,
            "email": email,
            "password": password,
            "confirmPassword": password,
        },
    )
    status, body = call("POST", "/Authorize/login", {"email": email, "password": password})
    if status != 200:
        raise RuntimeError(f"Could not log in as {email}: {status} {body}")
    login = value(body)
    return login["accessToken"] if "accessToken" in login else login["token"], login["user"]["id"]


def get_categories(token):
    status, body = call("GET", "/Categories", token=token)
    return value(body)


def random_date_in_last_month(rng):
    days_ago = rng.randint(0, 29)
    return (datetime.today() - timedelta(days=days_ago)).strftime("%Y-%m-%dT00:00:00")


def seed_transactions(token, categories, rng):
    expense_cats = [c for c in categories if c["type"] == "Expense" and c["name"] in EXPENSE_NOTES]
    income_cats = [c for c in categories if c["type"] == "Income" and c["name"] in INCOME_NOTES]
    count = 0

    # Salary near the start of the month, occasional side income.
    salary = next((c for c in income_cats if c["name"] == "เงินเดือน"), None)
    if salary:
        call(
            "POST",
            "/Transactions",
            {
                "categoryId": salary["id"],
                "type": "Income",
                "amount": rng.randint(18, 45) * 1000,
                "note": rng.choice(INCOME_NOTES["เงินเดือน"]),
                "transactionDate": (datetime.today().replace(day=1)).strftime("%Y-%m-%dT00:00:00"),
            },
            token,
        )
        count += 1

    if rng.random() < 0.5:
        extra = rng.choice([c for c in income_cats if c["name"] != "เงินเดือน"] or income_cats)
        call(
            "POST",
            "/Transactions",
            {
                "categoryId": extra["id"],
                "type": "Income",
                "amount": rng.randint(1, 8) * 1000,
                "note": rng.choice(INCOME_NOTES.get(extra["name"], ["รายได้เสริม"])),
                "transactionDate": random_date_in_last_month(rng),
            },
            token,
        )
        count += 1

    # ~24 expenses spread across the month and across every default category.
    for _ in range(24):
        cat = rng.choice(expense_cats)
        amount_range = {
            "อาหาร/เครื่องดื่ม": (40, 350),
            "ที่พักอาศัย": (2500, 9000),
            "การเดินทาง": (30, 500),
            "ช้อปปิ้ง": (150, 2500),
            "สุขภาพ": (100, 1500),
            "บันเทิง": (100, 900),
            "การศึกษา": (300, 3000),
            "ประกัน": (500, 2500),
            "สาธารณูปโภค": (200, 1800),
            "อื่นๆ (รายจ่าย)": (50, 500),
        }.get(cat["name"], (50, 500))

        call(
            "POST",
            "/Transactions",
            {
                "categoryId": cat["id"],
                "type": "Expense",
                "amount": rng.randint(*amount_range),
                "note": rng.choice(EXPENSE_NOTES[cat["name"]]),
                "transactionDate": random_date_in_last_month(rng),
            },
            token,
        )
        count += 1

    return count


def main():
    rng = random.Random(42)
    accounts = {}
    for email, first_name, last_name in USERS:
        password = ADMIN_PASSWORD if email == ADMIN_EMAIL else PASSWORD
        token, user_id = register_and_login(email, first_name, last_name, password)
        accounts[email] = {"token": token, "user_id": user_id}
        role = "Admin" if email == ADMIN_EMAIL else "User"
        print(f"user ready ({role}): {email} (id={user_id})")

    categories = get_categories(accounts[USERS[0][0]]["token"])
    print(f"{len(categories)} default categories available")

    print("\nseeding ~1 month of transactions per user...")
    for email in accounts:
        n = seed_transactions(accounts[email]["token"], categories, rng)
        print(f"  {email}: {n} transactions")

    system_expense = next(c for c in categories if c["type"] == "Expense" and c["name"] == "อาหาร/เครื่องดื่ม")

    def propose(from_email, to_email, note):
        status, body = call(
            "POST",
            "/TransactionProposals",
            {
                "recipientEmail": to_email,
                "type": "Expense",
                "amount": rng.randint(100, 800),
                "categoryId": system_expense["id"],
                "note": note,
                "transactionDate": datetime.today().strftime("%Y-%m-%dT00:00:00"),
            },
            accounts[from_email]["token"],
        )
        if status != 200:
            raise RuntimeError(f"propose {from_email}->{to_email} failed: {status} {body}")
        return value(body)

    print("\nseeding sample proposals (pending / accepted / rejected)...")

    # พ่อ -> pongpan: left pending, so the real account sees a badge + notification.
    pending_id = propose(USERS[1][0], USERS[0][0], "ค่าซ่อมแอร์บ้าน")
    print(f"  pending: {USERS[1][0]} -> {USERS[0][0]} (proposal #{pending_id})")

    # แม่ -> พี่: accepted, so พี่'s history shows a "(บันทึกโดย แม่)" transaction.
    accepted_id = propose(USERS[2][0], USERS[3][0], "ซื้อของฝากให้พี่")
    status, body = call(
        "POST",
        f"/TransactionProposals/{accepted_id}/accept",
        {},
        accounts[USERS[3][0]]["token"],
    )
    print(f"  accepted: {USERS[2][0]} -> {USERS[3][0]} (proposal #{accepted_id}, status={status})")

    # พี่ -> พ่อ: rejected, so พี่ gets a rejection notification.
    rejected_id = propose(USERS[3][0], USERS[1][0], "ค่าอาหารมื้อเย็น")
    status, body = call("POST", f"/TransactionProposals/{rejected_id}/reject", token=accounts[USERS[1][0]]["token"])
    print(f"  rejected: {USERS[3][0]} -> {USERS[1][0]} (proposal #{rejected_id}, status={status})")

    print("\n=== Done ===")
    print(f"Admin (pongpan): {ADMIN_EMAIL} / {ADMIN_PASSWORD}")
    print(f"Password for the other 3 family accounts: {PASSWORD}")
    for email, _, _ in USERS[1:]:
        print(f"  - {email}")


if __name__ == "__main__":
    main()

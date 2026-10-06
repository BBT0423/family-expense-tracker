using ClosedXML.Excel;
using Microsoft.Data.SqlClient;

// Exports the original web app's real transaction data as an .xlsx file shaped
// exactly like the desktop app's own Import wizard template
// (SummaryYearSalary.Web/src/views/budget/ImportView.vue: วันที่/ประเภท/หมวดหมู่/
// จำนวนเงิน/หมายเหตุ/สมาชิก), so it can be imported through the app's UI —
// no direct database writes at all.
internal static class ExportExcel
{
    private static readonly Dictionary<string, string> MemberNameByEmail = new()
    {
        ["paa@family.local"] = "paa",
        ["mae@family.local"] = "mae",
        ["noei@family.local"] = "noei",
        ["pongpan22120@gmail.com"] = "pongpan22120",
    };

    public static void Run(string sourceConnectionString, string outputPath)
    {
        using var source = new SqlConnection(sourceConnectionString);
        source.Open();

        using var workbook = new XLWorkbook();

        var txnCount = WriteTransactionsSheet(source, workbook);

        workbook.SaveAs(outputPath);

        Console.WriteLine($"Wrote {txnCount} transactions to:");
        Console.WriteLine($"  {outputPath}");
        Console.WriteLine();
        Console.WriteLine("Member names used in the 'สมาชิก' column: paa, mae, noei, pongpan22120");
        Console.WriteLine("Create matching members in Settings before importing (categories auto-create on import).");
    }

    private static int WriteTransactionsSheet(SqlConnection source, XLWorkbook workbook)
    {
        var sheet = workbook.Worksheets.Add("ธุรกรรม");
        sheet.Cell(1, 1).Value = "วันที่";
        sheet.Cell(1, 2).Value = "ประเภท";
        sheet.Cell(1, 3).Value = "หมวดหมู่";
        sheet.Cell(1, 4).Value = "จำนวนเงิน";
        sheet.Cell(1, 5).Value = "หมายเหตุ";
        sheet.Cell(1, 6).Value = "สมาชิก";

        using var cmd = source.CreateCommand();
        cmd.CommandText = @"
            SELECT t.TransactionDate, t.Type, c.Name AS CategoryName, t.Amount, t.Note, u.Email
            FROM Transactions t
            JOIN Categories c ON c.Id = t.CategoryId
            JOIN IdentityUsers u ON u.Id = t.UserId
            WHERE t.IsDeleted = 0
            ORDER BY t.TransactionDate, t.Id";
        using var reader = cmd.ExecuteReader();

        var row = 2;
        while (reader.Read())
        {
            var date = reader.GetDateTime(0);
            var type = reader.GetInt32(1);
            var categoryName = reader.GetString(2);
            var amount = reader.GetDecimal(3);
            var note = reader.IsDBNull(4) ? "" : reader.GetString(4);
            var email = reader.GetString(5);

            sheet.Cell(row, 1).Value = date.ToString("dd/MM/yyyy");
            sheet.Cell(row, 2).Value = type == 1 ? "รายรับ" : "รายจ่าย";
            sheet.Cell(row, 3).Value = categoryName;
            sheet.Cell(row, 4).Value = amount;
            sheet.Cell(row, 5).Value = note;
            sheet.Cell(row, 6).Value = MemberNameByEmail.GetValueOrDefault(email, email);
            row++;
        }

        sheet.Columns().AdjustToContents();
        return row - 2;
    }
}

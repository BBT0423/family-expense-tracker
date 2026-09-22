using System.Globalization;

using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

using ClosedXML.Excel;

namespace SummaryYearSalary.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public byte[] GenerateMonthlyReport(IEnumerable<Transaction> transactions, int month, int year)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("รายการธุรกรรม");

        // Header row
        string[] headers = ["วันที่", "ประเภท", "หมวดหมู่", "จำนวนเงิน (฿)", "หมายเหตุ"];
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4A90D9");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var row = 2;
        var list = transactions.OrderByDescending(t => t.TransactionDate).ToList();
        foreach (var t in list)
        {
            ws.Cell(row, 1).Value = t.TransactionDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            ws.Cell(row, 2).Value = t.Type == TransactionType.Income ? "รายรับ" : "รายจ่าย";
            ws.Cell(row, 3).Value = t.Category?.Name ?? string.Empty;
            ws.Cell(row, 4).Value = t.Amount;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = t.Note ?? string.Empty;

            if (t.Type == TransactionType.Income)
                ws.Cell(row, 4).Style.Font.FontColor = XLColor.FromHtml("#27ae60");
            else
                ws.Cell(row, 4).Style.Font.FontColor = XLColor.FromHtml("#e74c3c");

            row++;
        }

        // Summary rows
        row++;
        var totalIncome = list.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = list.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        ws.Cell(row, 3).Value = "รวมรายรับ";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = totalIncome;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Style.Font.FontColor = XLColor.FromHtml("#27ae60");
        ws.Cell(row, 4).Style.Font.Bold = true;

        row++;
        ws.Cell(row, 3).Value = "รวมรายจ่าย";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = totalExpense;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Style.Font.FontColor = XLColor.FromHtml("#e74c3c");
        ws.Cell(row, 4).Style.Font.Bold = true;

        row++;
        ws.Cell(row, 3).Value = "คงเหลือ";
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 4).Value = totalIncome - totalExpense;
        ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 4).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

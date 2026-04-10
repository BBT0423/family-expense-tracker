using SummaryYearSalary.Domain.Entities;

namespace SummaryYearSalary.Application.Interfaces;

public interface IExcelExportService
{
    byte[] GenerateMonthlyReport(IEnumerable<Transaction> transactions, int month, int year);
}

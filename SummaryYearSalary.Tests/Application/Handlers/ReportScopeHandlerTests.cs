using Moq;

using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Reports.Queries.GetMonthlySummaryQuery;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class ReportScopeHandlerTests : BaseTest
{
    private readonly Mock<ITransactionRepository> _transactions = new();

    private static Transaction Expense(string userId, decimal amount) => new()
    {
        UserId = userId,
        CreatedByUserId = userId,
        CategoryId = 1,
        Type = TransactionType.Expense,
        Amount = amount,
        TransactionDate = DateTime.Today,
        Category = new Category { Id = 1, Name = "อาหาร", Type = TransactionType.Expense }
    };

    private GetMonthlySummaryQueryHandler Handler() => new(_transactions.Object);

    [Fact]
    public async Task NoScope_UsesRequesterOwnData()
    {
        _transactions.Setup(r => r.GetByUserMonthYearAsync("owner", 9, 2026))
            .ReturnsAsync([Expense("owner", 100)]);

        var result = await Handler().Handle(new GetMonthlySummaryQuery("owner", 9, 2026), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(100, result.Value!.TotalExpense);
        _transactions.Verify(r => r.GetAllUsersMonthYearAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ScopeAll_AggregatesAcrossEveryUser()
    {
        _transactions.Setup(r => r.GetAllUsersMonthYearAsync(9, 2026))
            .ReturnsAsync([Expense("a", 100), Expense("b", 250)]);

        var result = await Handler().Handle(new GetMonthlySummaryQuery("admin", 9, 2026, "all"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(350, result.Value!.TotalExpense);
        _transactions.Verify(r => r.GetByUserMonthYearAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ScopeSpecificOtherUser_ReadsThatUsersDataNotRequesters()
    {
        _transactions.Setup(r => r.GetByUserMonthYearAsync("mother", 9, 2026))
            .ReturnsAsync([Expense("mother", 500)]);

        var result = await Handler().Handle(new GetMonthlySummaryQuery("admin", 9, 2026, "mother"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(500, result.Value!.TotalExpense);
        _transactions.Verify(r => r.GetByUserMonthYearAsync("admin", It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}

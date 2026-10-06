using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.BulkImportTransactionsCommand;

public class BulkImportTransactionsCommandHandler(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<BulkImportTransactionsCommand, Result<int>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<int>> Handle(BulkImportTransactionsCommand request, CancellationToken cancellationToken)
    {
        var categories = (await _categoryRepository.GetByUserIdAsync(request.UserId))
            .ToDictionary(c => c.Id);

        var transactions = new List<Transaction>();

        foreach (var item in request.Items)
        {
            if (!categories.ContainsKey(item.CategoryId))
                continue;

            transactions.Add(new Transaction
            {
                UserId = request.UserId,
                CreatedByUserId = request.UserId,
                CategoryId = item.CategoryId,
                MemberId = item.MemberId,
                Type = item.Type,
                Amount = item.Amount,
                Note = item.Note,
                TransactionDate = item.TransactionDate
            });
        }

        if (transactions.Count == 0)
            return Result.Failure<int>("ไม่มีรายการที่สามารถนำเข้าได้");

        await _transactionRepository.AddRangeAsync(transactions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(transactions.Count);
    }
}

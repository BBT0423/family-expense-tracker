using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.DeleteTransactionCommand;

public class DeleteTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteTransactionCommand, Result<bool>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id);
        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<bool>("ไม่พบรายการที่ต้องการลบ", 404);

        transaction.IsDeleted = true;
        await _transactionRepository.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}

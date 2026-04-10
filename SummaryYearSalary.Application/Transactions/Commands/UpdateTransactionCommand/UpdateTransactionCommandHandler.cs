using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Transactions.Responses;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.UpdateTransactionCommand;

public class UpdateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IFamilyMemberRepository memberRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateTransactionCommand, Result<TransactionResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IFamilyMemberRepository _memberRepository = memberRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<TransactionResponse>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id);
        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<TransactionResponse>("ไม่พบรายการที่ต้องการแก้ไข", 404);

        var member = await _memberRepository.GetByIdAsync(request.MemberId);
        if (member is null || member.UserId != request.UserId)
            return Result.Failure<TransactionResponse>("ไม่พบสมาชิกที่ระบุ", 404);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || (category.UserId != null && category.UserId != request.UserId))
            return Result.Failure<TransactionResponse>("ไม่พบหมวดหมู่ที่ระบุ", 404);

        transaction.MemberId = request.MemberId;
        transaction.CategoryId = request.CategoryId;
        transaction.Type = request.Type;
        transaction.Amount = request.Amount;
        transaction.Note = request.Note;
        transaction.TransactionDate = request.TransactionDate;

        await _transactionRepository.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new TransactionResponse(
            transaction.Id,
            member.Id, member.Name, member.Color,
            category.Id, category.Name, category.Icon, category.Color,
            transaction.Type.ToString(), transaction.Amount, transaction.Note,
            transaction.TransactionDate, transaction.CreatedAt));
    }
}

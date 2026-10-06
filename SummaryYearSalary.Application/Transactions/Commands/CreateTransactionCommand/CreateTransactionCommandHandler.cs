using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Transactions.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.CreateTransactionCommand;

public class CreateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateTransactionCommand, Result<TransactionResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<TransactionResponse>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || (category.UserId != null && category.UserId != request.UserId))
            return Result.Failure<TransactionResponse>("ไม่พบหมวดหมู่ที่ระบุ", 404);

        var member = request.MemberId.HasValue ? await _memberRepository.GetByIdAsync(request.MemberId.Value) : null;
        if (request.MemberId.HasValue && member is null)
            return Result.Failure<TransactionResponse>("ไม่พบสมาชิกที่ระบุ", 404);

        var transaction = new Transaction
        {
            UserId = request.UserId,
            CreatedByUserId = request.CreatedByUserId,
            CategoryId = request.CategoryId,
            MemberId = request.MemberId,
            Type = request.Type,
            Amount = request.Amount,
            Note = request.Note,
            TransactionDate = request.TransactionDate
        };

        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new TransactionResponse(
            transaction.Id,
            category.Id, category.Name, category.Icon, category.Color,
            transaction.Type.ToString(), transaction.Amount, transaction.Note,
            transaction.TransactionDate, transaction.CreatedAt,
            MemberId: member?.Id, MemberName: member?.Name, MemberColor: member?.Color));
    }
}

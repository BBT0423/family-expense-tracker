using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Transactions.Responses;
using SummaryYearSalary.Domain.Entities;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace SummaryYearSalary.Application.Transactions.Commands.CreateTransactionCommand;

public class CreateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IFamilyMemberRepository memberRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateTransactionCommand, Result<TransactionResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IFamilyMemberRepository _memberRepository = memberRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<TransactionResponse>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId);
        if (member is null || member.UserId != request.UserId)
            return Result.Failure<TransactionResponse>("ไม่พบสมาชิกที่ระบุ", 404);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || (category.UserId != null && category.UserId != request.UserId))
            return Result.Failure<TransactionResponse>("ไม่พบหมวดหมู่ที่ระบุ", 404);

        var transaction = new Transaction
        {
            UserId = request.UserId,
            MemberId = request.MemberId,
            CategoryId = request.CategoryId,
            Type = request.Type,
            Amount = request.Amount,
            Note = request.Note,
            TransactionDate = request.TransactionDate
        };

        await _transactionRepository.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new TransactionResponse(
            transaction.Id,
            member.Id, member.Name, member.Color,
            category.Id, category.Name, category.Icon, category.Color,
            transaction.Type.ToString(), transaction.Amount, transaction.Note,
            transaction.TransactionDate, transaction.CreatedAt));
    }
}

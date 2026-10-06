using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Application.Transactions.Responses;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

using MediatR;

namespace SummaryYearSalary.Application.Transactions.Commands.UpdateTransactionCommand;

public class UpdateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IMemberRepository memberRepository,
    ITransactionHistoryRepository transactionHistoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateTransactionCommand, Result<TransactionResponse>>
{
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IMemberRepository _memberRepository = memberRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository = transactionHistoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private static string TypeLabel(TransactionType type) => type == TransactionType.Income ? "รายรับ" : "รายจ่าย";

    public async Task<Result<TransactionResponse>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id);
        if (transaction is null || transaction.UserId != request.UserId)
            return Result.Failure<TransactionResponse>("ไม่พบรายการที่ต้องการแก้ไข", 404);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || (category.UserId != null && category.UserId != request.UserId))
            return Result.Failure<TransactionResponse>("ไม่พบหมวดหมู่ที่ระบุ", 404);

        var member = request.MemberId.HasValue ? await _memberRepository.GetByIdAsync(request.MemberId.Value) : null;
        if (request.MemberId.HasValue && member is null)
            return Result.Failure<TransactionResponse>("ไม่พบสมาชิกที่ระบุ", 404);

        // Record a field-level diff before the entity is overwritten, so a
        // popover can later show "who changed what from what to what" —
        // values are resolved to display text (names, not IDs) at write
        // time so the log still reads correctly even if a category/member
        // is later renamed or removed.
        var historyEntries = new List<TransactionHistory>();
        void AddHistory(string field, string? oldValue, string? newValue)
        {
            if (oldValue == newValue) return;
            historyEntries.Add(new TransactionHistory
            {
                TransactionId = transaction.Id,
                ChangedByUserId = request.UserId,
                FieldName = field,
                OldValue = oldValue,
                NewValue = newValue,
            });
        }

        if (transaction.CategoryId != request.CategoryId)
        {
            var oldCategory = await _categoryRepository.GetByIdAsync(transaction.CategoryId);
            AddHistory("หมวดหมู่", oldCategory?.Name, category.Name);
        }
        if (transaction.MemberId != request.MemberId)
        {
            var oldMember = transaction.MemberId.HasValue
                ? await _memberRepository.GetByIdAsync(transaction.MemberId.Value)
                : null;
            AddHistory("สมาชิก", oldMember?.Name, member?.Name);
        }
        if (transaction.Type != request.Type)
            AddHistory("ประเภท", TypeLabel(transaction.Type), TypeLabel(request.Type));
        if (transaction.Amount != request.Amount)
            AddHistory("จำนวนเงิน", transaction.Amount.ToString("N2"), request.Amount.ToString("N2"));
        if (transaction.Note != request.Note)
            AddHistory("หมายเหตุ", transaction.Note, request.Note);
        if (transaction.TransactionDate != request.TransactionDate)
            AddHistory("วันที่", transaction.TransactionDate.ToString("dd/MM/yyyy"), request.TransactionDate.ToString("dd/MM/yyyy"));

        transaction.CategoryId = request.CategoryId;
        transaction.MemberId = request.MemberId;
        transaction.Type = request.Type;
        transaction.Amount = request.Amount;
        transaction.Note = request.Note;
        transaction.TransactionDate = request.TransactionDate;

        await _transactionRepository.UpdateAsync(transaction);
        foreach (var entry in historyEntries)
            await _transactionHistoryRepository.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new TransactionResponse(
            transaction.Id,
            category.Id, category.Name, category.Icon, category.Color,
            transaction.Type.ToString(), transaction.Amount, transaction.Note,
            transaction.TransactionDate, transaction.CreatedAt,
            MemberId: member?.Id, MemberName: member?.Name, MemberColor: member?.Color));
    }
}

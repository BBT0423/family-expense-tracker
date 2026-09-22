using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.TransactionProposals.Commands.AcceptTransactionProposalCommand;

public class AcceptTransactionProposalCommandHandler(
    ITransactionProposalRepository proposalRepository,
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService,
    IUnitOfWork unitOfWork
) : IRequestHandler<AcceptTransactionProposalCommand, Result<bool>>
{
    private readonly ITransactionProposalRepository _proposalRepository = proposalRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(AcceptTransactionProposalCommand request, CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(request.ProposalId);
        if (proposal is null || proposal.OwnerUserId != request.UserId)
            return Result.Failure<bool>("ไม่พบรายการที่รอยืนยัน", 404);

        if (proposal.Status != TransactionProposalStatus.Pending)
            return Result.Failure<bool>("รายการนี้ถูกดำเนินการไปแล้ว", 409);

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId ?? proposal.CategoryId);
        if (category is null || (category.UserId is not null && category.UserId != request.UserId))
            return Result.Failure<bool>("ไม่พบหมวดหมู่ที่ระบุ", 404);

        if (category.Type != proposal.Type)
            return Result.Failure<bool>("ประเภทของหมวดหมู่ไม่ตรงกับประเภทรายการ", 400);

        await _transactionRepository.AddAsync(new Transaction
        {
            UserId = proposal.OwnerUserId,
            CreatedByUserId = proposal.ProposedByUserId,
            CategoryId = category.Id,
            Type = proposal.Type,
            Amount = proposal.Amount,
            Note = proposal.Note,
            TransactionDate = proposal.TransactionDate
        });

        proposal.Status = TransactionProposalStatus.Accepted;
        proposal.DecidedAt = DateTime.UtcNow;
        await _proposalRepository.UpdateAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var owner = await _userManager.FindByIdAsync(request.UserId);
        await _notificationService.NotifyAsync(
            proposal.ProposedByUserId,
            NotificationType.TransactionProposalAccepted,
            "รายการที่คุณลงให้ถูกยอมรับแล้ว",
            $"{UserDisplayName.Of(owner)} ยอมรับรายการ {category.Name} {proposal.Amount:N2} บาท ที่คุณลงให้",
            proposal.Id,
            cancellationToken);

        return Result.Success(true);
    }
}

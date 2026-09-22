using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.TransactionProposals.Commands.RejectTransactionProposalCommand;

public class RejectTransactionProposalCommandHandler(
    ITransactionProposalRepository proposalRepository,
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService,
    IUnitOfWork unitOfWork
) : IRequestHandler<RejectTransactionProposalCommand, Result<bool>>
{
    private readonly ITransactionProposalRepository _proposalRepository = proposalRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(RejectTransactionProposalCommand request, CancellationToken cancellationToken)
    {
        var proposal = await _proposalRepository.GetByIdAsync(request.ProposalId);
        if (proposal is null || proposal.OwnerUserId != request.UserId)
            return Result.Failure<bool>("ไม่พบรายการที่รอยืนยัน", 404);

        if (proposal.Status != TransactionProposalStatus.Pending)
            return Result.Failure<bool>("รายการนี้ถูกดำเนินการไปแล้ว", 409);

        proposal.Status = TransactionProposalStatus.Rejected;
        proposal.DecidedAt = DateTime.UtcNow;
        await _proposalRepository.UpdateAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var owner = await _userManager.FindByIdAsync(request.UserId);
        await _notificationService.NotifyAsync(
            proposal.ProposedByUserId,
            NotificationType.TransactionProposalRejected,
            "รายการที่คุณลงให้ถูกปฏิเสธ",
            $"{UserDisplayName.Of(owner)} ปฏิเสธรายการ {proposal.Amount:N2} บาท ที่คุณลงให้",
            proposal.Id,
            cancellationToken);

        return Result.Success(true);
    }
}

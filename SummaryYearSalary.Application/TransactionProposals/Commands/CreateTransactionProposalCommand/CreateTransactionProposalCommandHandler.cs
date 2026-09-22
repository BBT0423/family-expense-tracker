using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Application.TransactionProposals.Commands.CreateTransactionProposalCommand;

public class CreateTransactionProposalCommandHandler(
    ITransactionProposalRepository proposalRepository,
    ICategoryRepository categoryRepository,
    UserManager<ApplicationUser> userManager,
    INotificationService notificationService,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateTransactionProposalCommand, Result<int>>
{
    private readonly ITransactionProposalRepository _proposalRepository = proposalRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly INotificationService _notificationService = notificationService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<int>> Handle(CreateTransactionProposalCommand request, CancellationToken cancellationToken)
    {
        var owner = await _userManager.FindByEmailAsync(request.RecipientEmail.Trim());
        if (owner is null)
            return Result.Failure<int>("ไม่พบผู้ใช้ที่ใช้อีเมลนี้", 404);

        if (owner.Id == request.ProposedByUserId)
            return Result.Failure<int>("ไม่สามารถลงรายการให้ตัวเองผ่านช่องทางนี้ได้", 400);

        // Only system categories: user-owned categories are private and must not be exposed to other people.
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category is null || category.UserId is not null)
            return Result.Failure<int>("เลือกได้เฉพาะหมวดหมู่เริ่มต้นของระบบ", 400);

        if (category.Type != request.Type)
            return Result.Failure<int>("ประเภทของหมวดหมู่ไม่ตรงกับประเภทรายการ", 400);

        var proposal = new TransactionProposal
        {
            OwnerUserId = owner.Id,
            ProposedByUserId = request.ProposedByUserId,
            Type = request.Type,
            Amount = request.Amount,
            CategoryId = category.Id,
            Note = request.Note,
            TransactionDate = request.TransactionDate,
            Status = TransactionProposalStatus.Pending
        };

        await _proposalRepository.AddAsync(proposal);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var proposer = await _userManager.FindByIdAsync(request.ProposedByUserId);
        await _notificationService.NotifyAsync(
            owner.Id,
            NotificationType.TransactionProposed,
            "มีรายการรอให้คุณยืนยัน",
            $"{UserDisplayName.Of(proposer)} ลงรายการ {category.Name} {proposal.Amount:N2} บาท ให้คุณ กรุณาตรวจสอบและยอมรับ",
            proposal.Id,
            cancellationToken);

        return Result.Success(proposal.Id);
    }
}

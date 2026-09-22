using System.Net.Mail;

using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Domain.Enums;

using Microsoft.AspNetCore.Identity;

namespace SummaryYearSalary.Infrastructure.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IEmailRepository emailRepository
) : INotificationService
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IEmailRepository _emailRepository = emailRepository;

    public async Task NotifyAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        int? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityId = relatedEntityId,
            IsRead = false
        };

        await _notificationRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (string.IsNullOrWhiteSpace(user?.Email))
            return;

        using var mail = new MailMessage(new MailAddress(user.Email), new MailAddress(user.Email))
        {
            Subject = $"_SummaryYearSalary - {title}",
            Body = $"<p>{message}</p>",
            IsBodyHtml = true
        };

        await _emailRepository.SendEmailAsync(mail, cancellationToken);
    }
}

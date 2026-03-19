using SummaryYearSalary.Application.Common;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Authorize.Commands.SendPhoneOtpCommand;

public record SendPhoneOtpCommand(string PhoneNumber, string IpAddress, string Template = "") : IRequest<Result<bool>>;

public class SendPhoneOtpCommandValidator : AbstractValidator<SendPhoneOtpCommand>
{
    public SendPhoneOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^(\+66|0)\d{8,9}$").WithMessage("Phone number must be a valid Thai phone number.");
    }
}

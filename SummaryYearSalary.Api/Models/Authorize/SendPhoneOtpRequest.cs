using System.ComponentModel.DataAnnotations;

namespace SummaryYearSalary.Api.Models.Authorize;

public class SendPhoneOtpRequest
{
    [Required(ErrorMessage = "Phone number is required")]
    [RegularExpression(@"^(\+66|0)\d{8,9}$", ErrorMessage = "Phone number must be a valid Thai phone number")]
    public string PhoneNumber { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace SummaryYearSalary.Api.Models.Authorize;

public class VerifyPhoneOtpRequest
{
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "OTP code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP code must be 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must contain only digits")]
    public string OtpCode { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace SummaryYearSalary.Api.Models.Authorize;

public class VerifyMfaRequest
{
    [Required(ErrorMessage = "MFA code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "MFA code must be 6 digits")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "MFA code must be 6 digits")]
    public string MfaCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "UserId is required")]
    public string UserId { get; set; } = string.Empty;
}
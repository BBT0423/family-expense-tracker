namespace SummaryYearSalary.Application.User;

public record EnableMfaResponse(
    string SharedKey,
    string AuthenticatorUri
);
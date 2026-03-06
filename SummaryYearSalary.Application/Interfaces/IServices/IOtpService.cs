namespace SummaryYearSalary.Application.Interfaces.IServices;

public interface IOtpService
{
    string GenerateSecret();
    string GenerateTotp(string secret);
    bool VerifyTotp(string secret, string code, int windowSize = 1);
    string GenerateQrCodeUri(string email, string secret, string issuer = "SummaryYearSalary");
}
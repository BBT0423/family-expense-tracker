using SummaryYearSalary.Application.Interfaces.IServices;

using OtpNet;

namespace SummaryYearSalary.Infrastructure.Services;

public class OtpService : IOtpService
{
    public string GenerateSecret()
    {
        // Generate a random 20-byte secret (160 bits)
        var secret = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secret);
    }

    public string GenerateTotp(string secret)
    {
        var secretBytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(secretBytes);
        return totp.ComputeTotp();
    }

    public bool VerifyTotp(string secret, string code, int windowSize = 1)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);

            // Verify with time window (default: ±30 seconds)
            var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(windowSize, windowSize));

            return isValid;
        }
        catch
        {
            return false;
        }
    }

    public string GenerateQrCodeUri(string email, string secret, string issuer = "SummaryYearSalary")
    {
        // Generate otpauth:// URI for QR code
        return $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
    }
}
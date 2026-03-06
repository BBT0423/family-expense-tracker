using System.Text.Json;

namespace SummaryYearSalary.Tests.Helpers;

/// <summary>
/// Utility methods for testing.
/// Provides common test operations like serialization, assertions, etc.
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// Serialize an object to JSON string.
    /// </summary>
    public static string ToJson<T>(T obj)
    {
        return JsonSerializer.Serialize(obj);
    }

    /// <summary>
    /// Deserialize JSON string to object.
    /// </summary>
    public static T? FromJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json);
    }

    /// <summary>
    /// Check if JSON string contains a value.
    /// </summary>
    public static bool JsonContains(string json, string value)
    {
        return json.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Generate a random email address for testing.
    /// </summary>
    public static string GenerateRandomEmail()
    {
        return $"test-{Guid.NewGuid():N}@example.com";
    }

    /// <summary>
    /// Generate a random user ID for testing.
    /// </summary>
    public static string GenerateRandomUserId()
    {
        return $"user-{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Generate a random refresh token for testing.
    /// </summary>
    public static string GenerateRandomToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Generate a valid 6-digit MFA code.
    /// </summary>
    public static string GenerateValidMfaCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}

/// <summary>
/// Test data constants.
/// </summary>
public static class TestData
{
    public static class ValidEmails
    {
        public const string Primary = "test@example.com";
        public const string Secondary = "admin@example.com";
        public const string Development = "dev@example.local";
    }

    public static class ValidPasswords
    {
        public const string Strong = "StrongPassword@123";
        public const string Complex = "C0mpl3x!P@ssw0rd";
        public const string Secure = "SecurePass_2024!";
    }

    public static class InvalidEmails
    {
        public const string NoAtSign = "testexample.com";
        public const string NoDomain = "test@";
        public const string NoLocalPart = "@example.com";
        public const string Empty = "";
    }

    public static class InvalidPasswords
    {
        public const string TooShort = "Short1!";
        public const string NoSpecialChar = "Password123";
        public const string Empty = "";
        public const string Spaces = "Pass word 123";
    }

    public static class ValidTokens
    {
        public const string RefreshToken = "valid-refresh-token-12345";
        public const string ResetToken = "valid-reset-token-abcde";
        public const string MfaChallenge = "mfa-challenge-token-xyz";
    }

    public static class ValidMfaCodes
    {
        public const string Valid = "123456";
        public const string Another = "654321";
        public const string Zeros = "000000";
        public const string Nines = "999999";
    }

    public static class InvalidMfaCodes
    {
        public const string TooShort = "12345";
        public const string TooLong = "1234567";
        public const string WithLetters = "12345A";
        public const string WithSpecials = "123@56";
    }
}

/// <summary>
/// Test constants for routes and endpoints.
/// </summary>
public static class TestEndpoints
{
    public const string Login = "/api/authorize/login";
    public const string Refresh = "/api/authorize/refresh";
    public const string ForgotPassword = "/api/authorize/forgot-password";
    public const string ResetPassword = "/api/authorize/reset-password";
    public const string VerifyMfa = "/api/authorize/mfa/verify";
    public const string EnableMfa = "/api/user/mfa/enable";
    public const string ChangePassword = "/api/user/change-password";
}

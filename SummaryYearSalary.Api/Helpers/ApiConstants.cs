namespace SummaryYearSalary.Api.Helpers;

internal static class ApiConstants
{
    /// <summary>
    /// The name of the API.
    /// </summary>
    public const string ApiName = "SummaryYearSalary Api";

    /// <summary>
    /// The version of the API.
    /// </summary>
    public const string ApiVersion = "v1";

    /// <summary>
    /// The prefix for the API route.
    /// </summary>
    public const string ApiPrefix = $"api/{ApiVersion}";

    /// <summary>
    /// The name of the error object.
    /// </summary>
    public const string ErrorObjectName = "SummaryYearSalaryErrorObject";

    /// <summary>
    /// The ID of the error log.
    /// </summary>
    public const string ErrorLogId = "SummaryYearSalaryErrorLogId";

    /// <summary>
    /// Allow any origin.
    /// </summary>
    public const string WebApiPolicy = "allowWebApiWithOrigins";
    public const int MaxPageSize = 5000;

    public const string Unknown = "Unknown";
}
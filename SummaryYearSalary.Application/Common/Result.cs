namespace SummaryYearSalary.Application.Common;

public sealed class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public int StatusCode { get; private set; }
    public Dictionary<string, string[]> ValidationErrors { get; private set; }

    public Result(T value)
    {
        IsSuccess = true;
        Value = value;
        StatusCode = 200;
        Error = null;
        ValidationErrors = [];
    }

    public Result(string error, int statusCode, Dictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
        StatusCode = statusCode;
        ValidationErrors = validationErrors ?? [];
    }
}
public static class Result
{
    public static Result<T> Success<T>(T value) => new(value);
    public static Result<T> Failure<T>(string error, int statusCode = 400) => new(error, statusCode);
    public static Result<T> Unauthorized<T>(string error, int statusCode = 401) => new(error, statusCode);
    public static Result<T> ValidationFailure<T>(Dictionary<string, string[]> validationErrors) => new("Validation failed", 400, validationErrors);
}
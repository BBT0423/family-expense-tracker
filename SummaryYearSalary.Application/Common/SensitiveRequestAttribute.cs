namespace SummaryYearSalary.Application.Common;

/// <summary>
/// Marks a request whose payload must never be written to logs (passwords, tokens, one-time codes).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class SensitiveRequestAttribute : Attribute;

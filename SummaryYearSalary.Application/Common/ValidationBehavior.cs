using SummaryYearSalary.Application.Interfaces;

using FluentValidation;

using MediatR;

namespace SummaryYearSalary.Application.Common;

public class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest>? validator = null, IAppLogger? logger = null) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IValidator<TRequest>? _validator = validator;
    private readonly IAppLogger? _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

        // Skip logging for Authorize namespace requests to avoid logging sensitive data
        var requestType = typeof(TRequest);
        var shouldLog = !requestType.Namespace?.Contains("SummaryYearSalary.Application.Authorize", StringComparison.OrdinalIgnoreCase) ?? true;
        
        if (shouldLog)
        {
            _logger?.LogDebug("Request: {@Request}", request);
        }

        if (_validator is null)
        {
            return await next();
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        _logger?.LogDebug("Validation error: {@Errors}", errors);
        var resultType = typeof(TResponse);
        var result = Activator.CreateInstance(resultType, "Validation failed", 400, errors);
        return (TResponse)result!;
    }
}
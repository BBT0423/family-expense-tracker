using FluentValidation;

namespace SummaryYearSalary.Application.Common;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeGuid<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value => Guid.TryParse(value, out _))
                          .WithMessage("{PropertyName} must be a valid GUID.");
    }
}
using SummaryYearSalary.Application.Categories.Responses;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Categories.Queries.GetCategoriesQuery;

public class GetCategoriesQueryHandler(
    ICategoryRepository categoryRepository
) : IRequestHandler<GetCategoriesQuery, Result<IEnumerable<CategoryResponse>>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;

    public async Task<Result<IEnumerable<CategoryResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetByUserIdAsync(request.UserId, request.Type);
        var response = categories.Select(c => new CategoryResponse(
            c.Id, c.Name, c.Type.ToString(), c.Icon, c.Color, c.IsDefault));
        return Result.Success(response);
    }
}

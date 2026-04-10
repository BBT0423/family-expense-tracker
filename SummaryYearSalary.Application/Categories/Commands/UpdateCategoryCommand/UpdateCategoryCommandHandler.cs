using SummaryYearSalary.Application.Categories.Responses;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Categories.Commands.UpdateCategoryCommand;

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCategoryCommand, Result<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CategoryResponse>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category is null)
            return Result.Failure<CategoryResponse>("ไม่พบหมวดหมู่ที่ต้องการแก้ไข", 404);

        if (category.IsDefault || (category.UserId != null && category.UserId != request.UserId))
            return Result.Failure<CategoryResponse>("ไม่สามารถแก้ไขหมวดหมู่นี้ได้", 403);

        category.Name = request.Name;
        category.Type = request.Type;
        category.Icon = request.Icon;
        category.Color = request.Color;

        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CategoryResponse(
            category.Id, category.Name, category.Type.ToString(),
            category.Icon, category.Color, category.IsDefault));
    }
}

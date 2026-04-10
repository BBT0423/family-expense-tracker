using SummaryYearSalary.Application.Categories.Responses;
using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;
using SummaryYearSalary.Domain.Entities;

using MediatR;

namespace SummaryYearSalary.Application.Categories.Commands.CreateCategoryCommand;

public class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateCategoryCommand, Result<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CategoryResponse>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            UserId = request.UserId,
            Name = request.Name,
            Type = request.Type,
            Icon = request.Icon,
            Color = request.Color,
            IsDefault = false
        };

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CategoryResponse(
            category.Id, category.Name, category.Type.ToString(),
            category.Icon, category.Color, category.IsDefault));
    }
}

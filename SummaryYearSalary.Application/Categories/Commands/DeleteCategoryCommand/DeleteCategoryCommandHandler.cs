using SummaryYearSalary.Application.Common;
using SummaryYearSalary.Application.Interfaces;
using SummaryYearSalary.Application.Interfaces.IRepositories;

using MediatR;

namespace SummaryYearSalary.Application.Categories.Commands.DeleteCategoryCommand;

public class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.Id);
        if (category is null)
            return Result.Failure<bool>("ไม่พบหมวดหมู่ที่ต้องการลบ", 404);

        if (category.IsDefault)
            return Result.Failure<bool>("ไม่สามารถลบหมวดหมู่เริ่มต้นได้");

        if (category.UserId != request.UserId)
            return Result.Failure<bool>("ไม่สามารถลบหมวดหมู่นี้ได้", 403);

        var hasTransactions = await _categoryRepository.HasTransactionsAsync(request.Id);
        if (hasTransactions)
            return Result.Failure<bool>("ไม่สามารถลบหมวดหมู่ที่มีรายการอยู่ได้ กรุณาย้ายรายการก่อน", 409);

        category.IsDeleted = true;
        await _categoryRepository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}

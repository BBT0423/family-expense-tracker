using SummaryYearSalary.Api.Models.Budget;
using SummaryYearSalary.Application.Categories.Commands.CreateCategoryCommand;
using SummaryYearSalary.Application.Categories.Commands.UpdateCategoryCommand;
using SummaryYearSalary.Application.Transactions.Commands.BulkImportTransactionsCommand;
using SummaryYearSalary.Application.Transactions.Commands.CreateTransactionCommand;
using SummaryYearSalary.Application.Transactions.Commands.UpdateTransactionCommand;
using SummaryYearSalary.Domain.Enums;

using Mapster;

namespace SummaryYearSalary.Api.Configurations;

public class BudgetMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCategoryRequest, CreateCategoryCommand>()
            .Map(dest => dest.UserId, _ => string.Empty)
            .Map(dest => dest.Type, src => Enum.Parse<TransactionType>(src.Type, true));

        config.NewConfig<UpdateCategoryRequest, UpdateCategoryCommand>()
            .Map(dest => dest.UserId, _ => string.Empty)
            .Map(dest => dest.Id, _ => 0)
            .Map(dest => dest.Type, src => Enum.Parse<TransactionType>(src.Type, true));

        config.NewConfig<CreateTransactionRequest, CreateTransactionCommand>()
            .Map(dest => dest.UserId, _ => string.Empty)
            .Map(dest => dest.CreatedByUserId, _ => string.Empty)
            .Map(dest => dest.Type, src => Enum.Parse<TransactionType>(src.Type, true));

        config.NewConfig<UpdateTransactionRequest, UpdateTransactionCommand>()
            .Map(dest => dest.UserId, _ => string.Empty)
            .Map(dest => dest.Id, _ => 0)
            .Map(dest => dest.Type, src => Enum.Parse<TransactionType>(src.Type, true));

        config.NewConfig<BulkImportItemRequest, BulkImportItem>()
            .Map(dest => dest.Type, src => Enum.Parse<TransactionType>(src.Type, true));
    }
}

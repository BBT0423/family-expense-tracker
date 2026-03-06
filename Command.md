## Migration database
dotnet ef migrations add InitialCreate --project .\SummaryYearSalary.Infrastructure\ --startup-project .\SummaryYearSalary.Api\
dotnet ef database update --project .\SummaryYearSalary.Infrastructure\ --startup-project .\SummaryYearSalary.Api\


## Run Api Project
dotnet watch --project .\SummaryYearSalary.Api\
dotnet run --project .\SummaryYearSalary.Api\
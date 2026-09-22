namespace SummaryYearSalary.Api.Models.Users;

public class UpdateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

public class UpdateUserRoleRequest
{
    public required string Role { get; init; }
}

public class CreateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public required string Role { get; init; }
}

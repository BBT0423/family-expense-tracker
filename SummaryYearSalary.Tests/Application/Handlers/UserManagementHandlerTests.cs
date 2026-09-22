using Microsoft.AspNetCore.Identity;

using Moq;

using SummaryYearSalary.Application.Users.Commands.CreateUserCommand;
using SummaryYearSalary.Application.Users.Commands.UpdateUserCommand;
using SummaryYearSalary.Application.Users.Commands.UpdateUserRoleCommand;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class UserManagementHandlerTests : BaseTest
{
    private readonly Mock<UserManager<ApplicationUser>> _users = new(
        Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    // ---- Update ----

    [Fact]
    public async Task Update_UnknownUser_Returns404()
    {
        _users.Setup(u => u.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);
        var handler = new UpdateUserCommandHandler(_users.Object);

        var result = await handler.Handle(new UpdateUserCommand("missing", "A", "B"), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
        _users.Verify(u => u.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Update_Valid_ChangesNameAndSaves()
    {
        var target = new ApplicationUser { Id = "u1", FirstName = "Old", LastName = "Name" };
        _users.Setup(u => u.FindByIdAsync("u1")).ReturnsAsync(target);
        var handler = new UpdateUserCommandHandler(_users.Object);

        var result = await handler.Handle(new UpdateUserCommand("u1", "New", "Name2"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", target.FirstName);
        Assert.Equal("Name2", target.LastName);
        _users.Verify(u => u.UpdateAsync(target), Times.Once);
    }

    [Theory]
    [InlineData("", "L", false)]
    [InlineData("F", "L", true)]
    public void UpdateValidator_RequiresFirstName(string firstName, string lastName, bool valid)
    {
        var validator = new UpdateUserCommandValidator();

        Assert.Equal(valid, validator.Validate(new UpdateUserCommand("u1", firstName, lastName)).IsValid);
    }

    // ---- Create ----

    [Fact]
    public async Task Create_DuplicateEmail_Returns409()
    {
        _users.Setup(u => u.FindByEmailAsync("dup@x.com")).ReturnsAsync(new ApplicationUser { Id = "existing" });
        var handler = new CreateUserCommandHandler(_users.Object);

        var result = await handler.Handle(
            new CreateUserCommand("A", "B", "dup@x.com", "Passw0rd!x", "User"), CancellationToken.None);

        Assert.Equal(409, result.StatusCode);
        _users.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_Valid_CreatesUserAndAssignsRole()
    {
        _users.Setup(u => u.FindByEmailAsync("new@x.com")).ReturnsAsync((ApplicationUser?)null);
        ApplicationUser? created = null;
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), "Passw0rd!x"))
            .Callback<ApplicationUser, string>((u, _) => created = u)
            .ReturnsAsync(IdentityResult.Success);
        var handler = new CreateUserCommandHandler(_users.Object);

        var result = await handler.Handle(
            new CreateUserCommand("A", "B", "new@x.com", "Passw0rd!x", "User"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("new@x.com", created!.Email);
        _users.Verify(u => u.AddToRoleAsync(created, "User"), Times.Once);
    }

    [Fact]
    public async Task Create_IdentityFailure_ReturnsError()
    {
        _users.Setup(u => u.FindByEmailAsync("new@x.com")).ReturnsAsync((ApplicationUser?)null);
        _users.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "weak password" }));
        var handler = new CreateUserCommandHandler(_users.Object);

        var result = await handler.Handle(
            new CreateUserCommand("A", "B", "new@x.com", "weak", "User"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _users.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("bad-email", "Passw0rd!x", "User", false)]
    [InlineData("ok@x.com", "weak", "User", false)]
    [InlineData("ok@x.com", "Passw0rd!x", "SuperAdmin", false)]
    [InlineData("ok@x.com", "Passw0rd!x", "Admin", true)]
    public void CreateValidator_Rules(string email, string password, string role, bool valid)
    {
        var validator = new CreateUserCommandValidator();

        var result = validator.Validate(new CreateUserCommand("A", "B", email, password, role));

        Assert.Equal(valid, result.IsValid);
    }

    // ---- Update role ----

    [Fact]
    public async Task UpdateRole_UnknownUser_Returns404()
    {
        _users.Setup(u => u.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);
        var handler = new UpdateUserRoleCommandHandler(_users.Object);

        var result = await handler.Handle(new UpdateUserRoleCommand("admin", "missing", "Admin"), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task UpdateRole_SelfDemotion_Fails()
    {
        var target = new ApplicationUser { Id = "admin" };
        _users.Setup(u => u.FindByIdAsync("admin")).ReturnsAsync(target);
        var handler = new UpdateUserRoleCommandHandler(_users.Object);

        var result = await handler.Handle(new UpdateUserRoleCommand("admin", "admin", "User"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        _users.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRole_SelfKeepingAdmin_Succeeds()
    {
        var target = new ApplicationUser { Id = "admin" };
        _users.Setup(u => u.FindByIdAsync("admin")).ReturnsAsync(target);
        _users.Setup(u => u.GetRolesAsync(target)).ReturnsAsync(new List<string> { "Admin" });
        var handler = new UpdateUserRoleCommandHandler(_users.Object);

        var result = await handler.Handle(new UpdateUserRoleCommand("admin", "admin", "Admin"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateRole_Valid_ReplacesExistingRoles()
    {
        var target = new ApplicationUser { Id = "u1" };
        _users.Setup(u => u.FindByIdAsync("u1")).ReturnsAsync(target);
        _users.Setup(u => u.GetRolesAsync(target)).ReturnsAsync(new List<string> { "User" });
        var handler = new UpdateUserRoleCommandHandler(_users.Object);

        var result = await handler.Handle(new UpdateUserRoleCommand("admin", "u1", "Admin"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _users.Verify(u => u.RemoveFromRolesAsync(target, It.Is<IEnumerable<string>>(r => r.Contains("User"))), Times.Once);
        _users.Verify(u => u.AddToRoleAsync(target, "Admin"), Times.Once);
    }

    [Theory]
    [InlineData("User", true)]
    [InlineData("Admin", true)]
    [InlineData("SuperAdmin", false)]
    public void UpdateRoleValidator_OnlyAllowsKnownRoles(string role, bool valid)
    {
        var validator = new UpdateUserRoleCommandValidator();

        Assert.Equal(valid, validator.Validate(new UpdateUserRoleCommand("admin", "u1", role)).IsValid);
    }
}

using Microsoft.AspNetCore.Identity;

using Moq;

using SummaryYearSalary.Application.Users.Queries.GetUserDirectoryQuery;
using SummaryYearSalary.Domain.Entities;
using SummaryYearSalary.Tests.Fixtures;

namespace SummaryYearSalary.Tests.Application.Handlers;

public class UserDirectoryHandlerTests : BaseTest
{
    private readonly Mock<UserManager<ApplicationUser>> _users = new(
        Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

    [Fact]
    public async Task Handle_ExcludesRequesterAndReturnsNameAndEmailOnly()
    {
        var all = new List<ApplicationUser>
        {
            new() { Id = "me", FirstName = "Pongpan", Email = "me@x.com" },
            new() { Id = "father", FirstName = "พ่อ", LastName = "ทดสอบ", Email = "father@x.com" },
            new() { Id = "mother", FirstName = "แม่", LastName = "ทดสอบ", Email = "mother@x.com" },
        };
        _users.Setup(u => u.Users).Returns(all.AsQueryable());
        var handler = new GetUserDirectoryQueryHandler(_users.Object);

        var result = await handler.Handle(new GetUserDirectoryQuery("me"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var entries = result.Value!.ToList();
        Assert.Equal(2, entries.Count);
        Assert.DoesNotContain(entries, e => e.Id == "me");
        Assert.Contains(entries, e => e.Email == "father@x.com" && e.DisplayName == "พ่อ ทดสอบ");
        Assert.Contains(entries, e => e.Email == "mother@x.com" && e.DisplayName == "แม่ ทดสอบ");
    }
}

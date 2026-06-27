using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.ListUsers;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class ListUsersQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsMappedDtoPerUser()
    {
        var userA = new ApplicationUser { Id = Guid.NewGuid(), Email = "a@hotel.com", UserName = "a" };
        var userB = new ApplicationUser { Id = Guid.NewGuid(), Email = "b@hotel.com", UserName = "b" };

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.Users).Returns(new[] { userA, userB }.AsQueryable());
        userManager.Setup(m => m.GetRolesAsync(userA)).ReturnsAsync(["desk"]);
        userManager.Setup(m => m.GetRolesAsync(userB)).ReturnsAsync(["administrator"]);
        userManager.Setup(m => m.IsLockedOutAsync(userB)).ReturnsAsync(true);

        var handler = new ListUsersQueryHandler(userManager.Object);
        var result = await handler.HandleAsync(new ListUsersQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Single(u => u.Id == userA.Id).IsActive.ShouldBeTrue();
        result.Single(u => u.Id == userB.Id).IsActive.ShouldBeFalse();
    }
}

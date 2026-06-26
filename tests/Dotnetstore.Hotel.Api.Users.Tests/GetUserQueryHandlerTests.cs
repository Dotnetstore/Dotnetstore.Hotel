using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.GetUser;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class GetUserQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_UserExists_ReturnsMappedDto()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@hotel.com", UserName = "user@hotel.com" };

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["desk", "restaurant"]);

        var handler = new GetUserQueryHandler(userManager.Object);
        var result = await handler.HandleAsync(new GetUserQuery(user.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(user.Id);
        result.Email.ShouldBe("user@hotel.com");
        result.Roles.ShouldBe(["desk", "restaurant"]);
    }

    [Fact]
    public async Task HandleAsync_UserDoesNotExist_ReturnsNull()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserQueryHandler(userManager.Object);
        var result = await handler.HandleAsync(new GetUserQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}

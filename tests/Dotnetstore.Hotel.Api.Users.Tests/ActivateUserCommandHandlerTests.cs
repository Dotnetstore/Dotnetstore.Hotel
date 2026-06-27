using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.ActivateUser;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class ActivateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingUser_ClearsLockoutAndReturnsTrue()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@hotel.com", UserName = "user" };

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.SetLockoutEndDateAsync(user, null)).ReturnsAsync(IdentityResult.Success);

        var handler = new ActivateUserCommandHandler(userManager.Object);
        var result = await handler.HandleAsync(new ActivateUserCommand(user.Id), CancellationToken.None);

        result.ShouldBeTrue();
        userManager.Verify(m => m.SetLockoutEndDateAsync(user, null), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsFalse()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var handler = new ActivateUserCommandHandler(userManager.Object);
        var result = await handler.HandleAsync(new ActivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeFalse();
    }
}

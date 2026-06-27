using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.UpdateUser;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class UpdateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidRequest_UpdatesProfileRoleAndPassword()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "old@hotel.com", UserName = "old" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("restaurant")).ReturnsAsync(true);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.SetUserNameAsync(user, "newname")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.SetEmailAsync(user, "new@hotel.com")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["desk"]);
        userManager.Setup(m => m.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("desk")))).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(user, "restaurant")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-token");
        userManager.Setup(m => m.ResetPasswordAsync(user, "reset-token", "NewPass1!")).ReturnsAsync(IdentityResult.Success);

        var handler = new UpdateUserCommandHandler(userManager.Object, roleManager.Object);
        var command = new UpdateUserCommand(user.Id, "new@hotel.com", "newname", "restaurant", "NewPass1!");
        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Errors.ShouldBeEmpty();
        result.User.ShouldNotBeNull();
        result.User.Roles.ShouldBe(["restaurant"]);
        userManager.Verify(m => m.ResetPasswordAsync(user, "reset-token", "NewPass1!"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoPasswordProvided_SkipsPasswordReset()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "old@hotel.com", UserName = "old" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("desk")).ReturnsAsync(true);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.SetUserNameAsync(user, "old")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.SetEmailAsync(user, "old@hotel.com")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync([]);
        userManager.Setup(m => m.AddToRoleAsync(user, "desk")).ReturnsAsync(IdentityResult.Success);

        var handler = new UpdateUserCommandHandler(userManager.Object, roleManager.Object);
        var command = new UpdateUserCommand(user.Id, "old@hotel.com", "old", "desk", null);
        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Errors.ShouldBeEmpty();
        userManager.Verify(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UnknownRole_ReturnsError()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("unknown")).ReturnsAsync(false);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new ApplicationUser { Id = Guid.NewGuid() });

        var handler = new UpdateUserCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new UpdateUserCommand(Guid.NewGuid(), "a@hotel.com", "a", "unknown", null), CancellationToken.None);

        result.User.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsError()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var handler = new UpdateUserCommandHandler(userManager.Object, IdentityMocks.CreateRoleManagerMock().Object);
        var result = await handler.HandleAsync(new UpdateUserCommand(Guid.NewGuid(), "a@hotel.com", "a", "desk", null), CancellationToken.None);

        result.User.ShouldBeNull();
        result.Errors.ShouldContain("User not found.");
    }
}

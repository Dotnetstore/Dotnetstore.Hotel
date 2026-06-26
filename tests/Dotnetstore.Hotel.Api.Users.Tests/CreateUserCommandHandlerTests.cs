using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.CreateUser;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesUserAndAssignsRole()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("desk")).ReturnsAsync(true);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Password1!")).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "desk")).ReturnsAsync(IdentityResult.Success);

        var handler = new CreateUserCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new CreateUserCommand("new@hotel.com", "newuser", "Password1!", "desk"), CancellationToken.None);

        result.Errors.ShouldBeEmpty();
        result.User.ShouldNotBeNull();
        result.User.Email.ShouldBe("new@hotel.com");
        result.User.Roles.ShouldBe(["desk"]);
    }

    [Fact]
    public async Task HandleAsync_UnknownRole_ReturnsError()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("unknown")).ReturnsAsync(false);

        var userManager = IdentityMocks.CreateUserManagerMock();

        var handler = new CreateUserCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new CreateUserCommand("new@hotel.com", "newuser", "Password1!", "unknown"), CancellationToken.None);

        result.User.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        userManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsErrors()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("desk")).ReturnsAsync(true);

        var userManager = IdentityMocks.CreateUserManagerMock();
        var failure = IdentityResult.Failed(new IdentityError { Description = "Email already taken." });
        userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(failure);

        var handler = new CreateUserCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new CreateUserCommand("dup@hotel.com", "dupuser", "Password1!", "desk"), CancellationToken.None);

        result.User.ShouldBeNull();
        result.Errors.ShouldContain("Email already taken.");
    }
}

using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.UpdateRole;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class UpdateRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidRename_UpdatesRole()
    {
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "desk" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.RoleExistsAsync("front-desk")).ReturnsAsync(false);
        roleManager.Setup(m => m.SetRoleNameAsync(role, "front-desk"))
            .Callback<ApplicationRole, string>((r, name) => r.Name = name)
            .ReturnsAsync(IdentityResult.Success);
        roleManager.Setup(m => m.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.GetUsersInRoleAsync("front-desk")).ReturnsAsync([]);

        var handler = new UpdateRoleCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new UpdateRoleCommand(role.Id, "front-desk"), CancellationToken.None);

        result.Errors.ShouldBeEmpty();
        result.Role.ShouldNotBeNull();
        result.Role.Name.ShouldBe("front-desk");
    }

    [Theory]
    [InlineData("administrator")]
    [InlineData("superuser")]
    public async Task HandleAsync_ProtectedRole_ReturnsError(string protectedName)
    {
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = protectedName };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new UpdateRoleCommandHandler(IdentityMocks.CreateUserManagerMock().Object, roleManager.Object);
        var result = await handler.HandleAsync(new UpdateRoleCommand(role.Id, "renamed"), CancellationToken.None);

        result.Role.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        roleManager.Verify(m => m.SetRoleNameAsync(It.IsAny<ApplicationRole>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RoleNotFound_ReturnsError()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationRole?)null);

        var handler = new UpdateRoleCommandHandler(IdentityMocks.CreateUserManagerMock().Object, roleManager.Object);
        var result = await handler.HandleAsync(new UpdateRoleCommand(Guid.NewGuid(), "anything"), CancellationToken.None);

        result.Role.ShouldBeNull();
        result.Errors.ShouldContain("Role not found.");
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsError()
    {
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "desk" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.RoleExistsAsync("restaurant")).ReturnsAsync(true);

        var handler = new UpdateRoleCommandHandler(IdentityMocks.CreateUserManagerMock().Object, roleManager.Object);
        var result = await handler.HandleAsync(new UpdateRoleCommand(role.Id, "restaurant"), CancellationToken.None);

        result.Role.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        roleManager.Verify(m => m.SetRoleNameAsync(It.IsAny<ApplicationRole>(), It.IsAny<string>()), Times.Never);
    }
}

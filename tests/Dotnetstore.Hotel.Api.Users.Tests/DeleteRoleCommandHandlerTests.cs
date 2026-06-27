using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.DeleteRole;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class DeleteRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_UnusedRole_DeletesSuccessfully()
    {
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "housekeeping" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);
        roleManager.Setup(m => m.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.GetUsersInRoleAsync("housekeeping")).ReturnsAsync([]);

        var handler = new DeleteRoleCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new DeleteRoleCommand(role.Id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_RoleInUse_ReturnsErrorWithCount()
    {
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "desk" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.GetUsersInRoleAsync("desk")).ReturnsAsync([new ApplicationUser(), new ApplicationUser()]);

        var handler = new DeleteRoleCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new DeleteRoleCommand(role.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.Single().ShouldContain("2");
        roleManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationRole>()), Times.Never);
    }

    [Theory]
    [InlineData("administrator")]
    [InlineData("superuser")]
    public async Task HandleAsync_ProtectedRole_ReturnsError(string protectedName)
    {
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = protectedName };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var handler = new DeleteRoleCommandHandler(IdentityMocks.CreateUserManagerMock().Object, roleManager.Object);
        var result = await handler.HandleAsync(new DeleteRoleCommand(role.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        roleManager.Verify(m => m.DeleteAsync(It.IsAny<ApplicationRole>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RoleNotFound_ReturnsError()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationRole?)null);

        var handler = new DeleteRoleCommandHandler(IdentityMocks.CreateUserManagerMock().Object, roleManager.Object);
        var result = await handler.HandleAsync(new DeleteRoleCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("Role not found.");
    }
}

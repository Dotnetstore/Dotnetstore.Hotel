using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.CreateRole;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class CreateRoleCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidName_CreatesRole()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("housekeeping")).ReturnsAsync(false);
        roleManager.Setup(m => m.CreateAsync(It.Is<ApplicationRole>(r => r.Name == "housekeeping"))).ReturnsAsync(IdentityResult.Success);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.GetUsersInRoleAsync("housekeeping")).ReturnsAsync([]);

        var handler = new CreateRoleCommandHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new CreateRoleCommand("housekeeping"), CancellationToken.None);

        result.Errors.ShouldBeEmpty();
        result.Role.ShouldNotBeNull();
        result.Role.Name.ShouldBe("housekeeping");
        result.Role.UserCount.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsError()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.RoleExistsAsync("desk")).ReturnsAsync(true);

        var handler = new CreateRoleCommandHandler(IdentityMocks.CreateUserManagerMock().Object, roleManager.Object);
        var result = await handler.HandleAsync(new CreateRoleCommand("desk"), CancellationToken.None);

        result.Role.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        roleManager.Verify(m => m.CreateAsync(It.IsAny<ApplicationRole>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BlankName_ReturnsError()
    {
        var handler = new CreateRoleCommandHandler(IdentityMocks.CreateUserManagerMock().Object, IdentityMocks.CreateRoleManagerMock().Object);
        var result = await handler.HandleAsync(new CreateRoleCommand("   "), CancellationToken.None);

        result.Role.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
    }
}

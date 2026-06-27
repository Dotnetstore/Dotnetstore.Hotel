using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.GetRole;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class GetRoleQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_RoleExists_ReturnsMappedDto()
    {
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "desk" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(role.Id.ToString())).ReturnsAsync(role);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.GetUsersInRoleAsync("desk")).ReturnsAsync([]);

        var handler = new GetRoleQueryHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new GetRoleQuery(role.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("desk");
        result.IsProtected.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_RoleNotFound_ReturnsNull()
    {
        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationRole?)null);

        var handler = new GetRoleQueryHandler(IdentityMocks.CreateUserManagerMock().Object, roleManager.Object);
        var result = await handler.HandleAsync(new GetRoleQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}

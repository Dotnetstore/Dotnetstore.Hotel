using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.ListRoles;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class ListRolesQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsMappedDtoPerRole()
    {
        var adminRole = new ApplicationRole { Id = Guid.NewGuid(), Name = "administrator" };
        var deskRole = new ApplicationRole { Id = Guid.NewGuid(), Name = "desk" };

        var roleManager = IdentityMocks.CreateRoleManagerMock();
        roleManager.Setup(m => m.Roles).Returns(new[] { adminRole, deskRole }.AsQueryable());

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.GetUsersInRoleAsync("administrator")).ReturnsAsync([new ApplicationUser()]);
        userManager.Setup(m => m.GetUsersInRoleAsync("desk")).ReturnsAsync([]);

        var handler = new ListRolesQueryHandler(userManager.Object, roleManager.Object);
        var result = await handler.HandleAsync(new ListRolesQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        var admin = result.Single(r => r.Name == "administrator");
        admin.UserCount.ShouldBe(1);
        admin.IsProtected.ShouldBeTrue();
        var desk = result.Single(r => r.Name == "desk");
        desk.UserCount.ShouldBe(0);
        desk.IsProtected.ShouldBeFalse();
    }
}

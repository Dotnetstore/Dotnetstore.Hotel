using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.DeleteRole;

public class DeleteRoleCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : ICommandHandler<DeleteRoleCommand, DeleteRoleResponse>
{
    public async Task<DeleteRoleResponse> HandleAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString());
        if (role is null)
        {
            return new DeleteRoleResponse(false, ["Role not found."]);
        }

        var roleName = role.Name ?? string.Empty;
        if (Roles.IsProtected(roleName))
        {
            return new DeleteRoleResponse(false, [$"Role '{roleName}' cannot be deleted."]);
        }

        var usersInRole = await userManager.GetUsersInRoleAsync(roleName);
        if (usersInRole.Count > 0)
        {
            return new DeleteRoleResponse(false, [$"{usersInRole.Count} user(s) still have this role. Reassign them before deleting it."]);
        }

        var deleteResult = await roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
        {
            return new DeleteRoleResponse(false, deleteResult.Errors.Select(e => e.Description).ToList());
        }

        return new DeleteRoleResponse(true, []);
    }
}

using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.UpdateRole;

public class UpdateRoleCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : ICommandHandler<UpdateRoleCommand, UpdateRoleResponse>
{
    public async Task<UpdateRoleResponse> HandleAsync(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(command.Id.ToString());
        if (role is null)
        {
            return new UpdateRoleResponse(null, ["Role not found."]);
        }

        if (Roles.IsProtected(role.Name ?? string.Empty))
        {
            return new UpdateRoleResponse(null, [$"Role '{role.Name}' cannot be renamed."]);
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new UpdateRoleResponse(null, ["Role name is required."]);
        }

        var nameChanged = !string.Equals(role.Name, command.Name, StringComparison.OrdinalIgnoreCase);
        if (nameChanged && await roleManager.RoleExistsAsync(command.Name))
        {
            return new UpdateRoleResponse(null, [$"Role '{command.Name}' already exists."]);
        }

        var renameResult = await roleManager.SetRoleNameAsync(role, command.Name);
        if (!renameResult.Succeeded)
        {
            return new UpdateRoleResponse(null, renameResult.Errors.Select(e => e.Description).ToList());
        }

        var updateResult = await roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            return new UpdateRoleResponse(null, updateResult.Errors.Select(e => e.Description).ToList());
        }

        var dto = await RoleDtoMapper.ToDtoAsync(userManager, role);
        return new UpdateRoleResponse(dto, []);
    }
}

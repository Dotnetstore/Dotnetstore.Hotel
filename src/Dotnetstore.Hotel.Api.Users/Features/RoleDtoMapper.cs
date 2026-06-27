using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features;

/// <summary>
/// Shared "build a RoleDto for this role" mapping, used by every Roles feature that returns one
/// (Create/Get/List/Update) so the UserCount/IsProtected lookup stays in one place.
/// </summary>
internal static class RoleDtoMapper
{
    public static async Task<RoleDto> ToDtoAsync(UserManager<ApplicationUser> userManager, ApplicationRole role)
    {
        var name = role.Name ?? string.Empty;
        var usersInRole = await userManager.GetUsersInRoleAsync(name);
        return new RoleDto(role.Id, name, usersInRole.Count, Roles.IsProtected(name));
    }
}

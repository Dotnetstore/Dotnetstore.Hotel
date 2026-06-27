using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.GetRole;

public class GetRoleQueryHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : IQueryHandler<GetRoleQuery, RoleDto?>
{
    public async Task<RoleDto?> HandleAsync(GetRoleQuery query, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(query.Id.ToString());
        if (role is null)
        {
            return null;
        }

        return await RoleDtoMapper.ToDtoAsync(userManager, role);
    }
}

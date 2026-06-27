using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Roles;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.ListRoles;

public class ListRolesQueryHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleDto>>
{
    public async Task<IReadOnlyList<RoleDto>> HandleAsync(ListRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = roleManager.Roles.ToList();

        var result = new List<RoleDto>(roles.Count);
        foreach (var role in roles)
        {
            result.Add(await RoleDtoMapper.ToDtoAsync(userManager, role));
        }

        return result;
    }
}

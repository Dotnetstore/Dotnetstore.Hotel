using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.ListUsers;

public class ListUsersQueryHandler(UserManager<ApplicationUser> userManager) : IQueryHandler<ListUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> HandleAsync(ListUsersQuery query, CancellationToken cancellationToken)
    {
        // Plain synchronous materialization (not EF's ToListAsync) keeps UserManager.Users mockable
        // with a simple in-memory IQueryable in unit tests, without needing a real DbContext/async query provider.
        var users = userManager.Users.ToList();

        var result = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(await UserDtoMapper.ToDtoAsync(userManager, user, roles.ToList()));
        }

        return result;
    }
}

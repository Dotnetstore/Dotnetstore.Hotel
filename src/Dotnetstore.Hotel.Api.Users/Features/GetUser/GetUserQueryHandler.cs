using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.GetUser;

public class GetUserQueryHandler(UserManager<ApplicationUser> userManager) : IQueryHandler<GetUserQuery, UserDto?>
{
    public async Task<UserDto?> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(query.Id.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new UserDto(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, roles.ToList());
    }
}

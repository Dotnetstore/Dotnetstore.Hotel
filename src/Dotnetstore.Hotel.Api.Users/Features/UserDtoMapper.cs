using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features;

/// <summary>
/// Shared "build a UserDto for this user" mapping, used by every Users feature that returns one
/// (Create/Get/List/Update) so the IsActive/lockout lookup stays in one place.
/// </summary>
internal static class UserDtoMapper
{
    public static async Task<UserDto> ToDtoAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        var isActive = !await userManager.IsLockedOutAsync(user);
        return new UserDto(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, roles, isActive);
    }
}

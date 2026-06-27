using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Shared.Cqrs;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.ActivateUser;

public class ActivateUserCommandHandler(UserManager<ApplicationUser> userManager) : ICommandHandler<ActivateUserCommand, bool>
{
    public async Task<bool> HandleAsync(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.Id.ToString());
        if (user is null)
        {
            return false;
        }

        await userManager.SetLockoutEndDateAsync(user, null);
        return true;
    }
}

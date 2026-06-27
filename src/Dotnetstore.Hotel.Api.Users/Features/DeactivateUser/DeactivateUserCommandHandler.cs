using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Microsoft.AspNetCore.Identity;

namespace Dotnetstore.Hotel.Api.Users.Features.DeactivateUser;

public class DeactivateUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeactivateUserCommand, bool>
{
    public async Task<bool> HandleAsync(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == command.RequestedByUserId)
        {
            return false;
        }

        var user = await userManager.FindByIdAsync(command.Id.ToString());
        if (user is null)
        {
            return false;
        }

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

        var activeTokens = await refreshTokenRepository.GetActiveByUserIdAsync(command.Id, cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        if (activeTokens.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}

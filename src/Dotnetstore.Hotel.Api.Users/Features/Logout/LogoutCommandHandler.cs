using Dotnetstore.Hotel.Api.Users.Authentication;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Users.Features.Logout;

public class LogoutCommandHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<LogoutCommand, bool>
{
    public async Task<bool> HandleAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenGenerator.Hash(command.RefreshToken);
        var token = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (token is null || !token.IsActive)
        {
            return false;
        }

        token.Revoke();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

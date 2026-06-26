using Dotnetstore.Hotel.Api.Users.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Users.Features.RevokeUserTokens;

public class RevokeUserTokensCommandHandler(IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<RevokeUserTokensCommand, int>
{
    public async Task<int> HandleAsync(RevokeUserTokensCommand command, CancellationToken cancellationToken)
    {
        var activeTokens = await refreshTokenRepository.GetActiveByUserIdAsync(command.UserId, cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke();
        }

        if (activeTokens.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return activeTokens.Count;
    }
}

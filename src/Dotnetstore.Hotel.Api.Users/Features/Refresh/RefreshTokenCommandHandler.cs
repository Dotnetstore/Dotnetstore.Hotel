using Dotnetstore.Hotel.Api.Users.Authentication;
using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Dotnetstore.Hotel.Api.Users.Features.Refresh;

public class RefreshTokenCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings)
    : ICommandHandler<RefreshTokenCommand, LoginResponse?>
{
    public async Task<LoginResponse?> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenGenerator.Hash(command.RefreshToken);
        var existingToken = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, accessTokenExpiresAtUtc) = jwtTokenService.GenerateToken(user, roles.ToList());

        var (rawRefreshToken, refreshTokenHash) = RefreshTokenGenerator.Generate();
        var refreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.Add(jwtSettings.Value.RefreshTokenExpiry);
        var newToken = RefreshToken.Create(user.Id, refreshTokenHash, refreshTokenExpiresAtUtc);
        await refreshTokenRepository.AddAsync(newToken, cancellationToken);

        existingToken.Revoke(newToken.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(accessToken, accessTokenExpiresAtUtc, rawRefreshToken, refreshTokenExpiresAtUtc);
    }
}

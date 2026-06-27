using Dotnetstore.Hotel.Api.Users.Authentication;
using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Dotnetstore.Hotel.Api.Users.Features.Login;

public class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtSettings> jwtSettings)
    : ICommandHandler<LoginCommand, LoginResponse?>
{
    public async Task<LoginResponse?> HandleAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            return null;
        }

        var passwordValid = await userManager.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
        {
            return null;
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = jwtTokenService.GenerateToken(user, roles.ToList());

        var (rawRefreshToken, refreshTokenHash) = RefreshTokenGenerator.Generate();
        var refreshTokenExpiresAtUtc = DateTimeOffset.UtcNow.Add(jwtSettings.Value.RefreshTokenExpiry);
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash, refreshTokenExpiresAtUtc);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResponse(token, expiresAtUtc, rawRefreshToken, refreshTokenExpiresAtUtc);
    }
}

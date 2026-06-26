using Dotnetstore.Hotel.Api.Users.Authentication;
using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.Refresh;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class RefreshTokenCommandHandlerTests
{
    private static readonly IOptions<JwtSettings> DefaultJwtSettings = Options.Create(new JwtSettings { SigningKey = "test" });

    [Fact]
    public async Task HandleAsync_ValidActiveToken_RotatesAndReturnsNewPair()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@hotel.com", UserName = "user@hotel.com" };
        var existingToken = RefreshToken.Create(user.Id, "existing-hash", DateTimeOffset.UtcNow.AddDays(1));

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["desk"]);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(s => s.GenerateToken(user, It.IsAny<IReadOnlyCollection<string>>())).Returns(("new-access-token", expiresAt));

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RefreshTokenCommandHandler(userManager.Object, jwtTokenService.Object, refreshTokenRepository.Object, unitOfWork.Object, DefaultJwtSettings);
        var result = await handler.HandleAsync(new RefreshTokenCommand("raw-token"), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Token.ShouldBe("new-access-token");
        existingToken.IsActive.ShouldBeFalse();
        existingToken.RevokedAtUtc.ShouldNotBeNull();
        existingToken.ReplacedByTokenId.ShouldNotBeNull();
        refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UnknownToken_ReturnsNull()
    {
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var handler = new RefreshTokenCommandHandler(
            IdentityMocks.CreateUserManagerMock().Object,
            Mock.Of<IJwtTokenService>(),
            refreshTokenRepository.Object,
            Mock.Of<IUnitOfWork>(),
            DefaultJwtSettings);

        var result = await handler.HandleAsync(new RefreshTokenCommand("unknown-token"), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_ExpiredToken_ReturnsNull()
    {
        var expiredToken = RefreshToken.Create(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(-1));

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(expiredToken);

        var handler = new RefreshTokenCommandHandler(
            IdentityMocks.CreateUserManagerMock().Object,
            Mock.Of<IJwtTokenService>(),
            refreshTokenRepository.Object,
            Mock.Of<IUnitOfWork>(),
            DefaultJwtSettings);

        var result = await handler.HandleAsync(new RefreshTokenCommand("expired-token"), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_AlreadyRevokedToken_ReturnsNull()
    {
        var revokedToken = RefreshToken.Create(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(1));
        revokedToken.Revoke();

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(revokedToken);

        var handler = new RefreshTokenCommandHandler(
            IdentityMocks.CreateUserManagerMock().Object,
            Mock.Of<IJwtTokenService>(),
            refreshTokenRepository.Object,
            Mock.Of<IUnitOfWork>(),
            DefaultJwtSettings);

        var result = await handler.HandleAsync(new RefreshTokenCommand("revoked-token"), CancellationToken.None);

        result.ShouldBeNull();
    }
}

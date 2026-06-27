using Dotnetstore.Hotel.Api.Users.Authentication;
using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.Login;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class LoginCommandHandlerTests
{
    private static readonly IOptions<JwtSettings> DefaultJwtSettings = Options.Create(new JwtSettings { SigningKey = "test" });

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsTokenAndPersistsRefreshToken()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@hotel.com", UserName = "user@hotel.com" };

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(["desk"]);

        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        var jwtTokenService = new Mock<IJwtTokenService>();
        jwtTokenService.Setup(s => s.GenerateToken(user, It.Is<IReadOnlyCollection<string>>(r => r.Contains("desk"))))
            .Returns(("token-value", expiresAt));

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new LoginCommandHandler(userManager.Object, jwtTokenService.Object, refreshTokenRepository.Object, unitOfWork.Object, DefaultJwtSettings);
        var result = await handler.HandleAsync(new LoginCommand(user.Email, "correct-password"), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Token.ShouldBe("token-value");
        result.ExpiresAtUtc.ShouldBe(expiresAt);
        result.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        refreshTokenRepository.Verify(r => r.AddAsync(It.Is<RefreshToken>(t => t.UserId == user.Id), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UnknownEmail_ReturnsNull()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var handler = new LoginCommandHandler(userManager.Object, Mock.Of<IJwtTokenService>(), Mock.Of<IRefreshTokenRepository>(), Mock.Of<IUnitOfWork>(), DefaultJwtSettings);
        var result = await handler.HandleAsync(new LoginCommand("missing@hotel.com", "anything"), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsNull()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@hotel.com", UserName = "user@hotel.com" };

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "wrong-password")).ReturnsAsync(false);

        var handler = new LoginCommandHandler(userManager.Object, Mock.Of<IJwtTokenService>(), Mock.Of<IRefreshTokenRepository>(), Mock.Of<IUnitOfWork>(), DefaultJwtSettings);
        var result = await handler.HandleAsync(new LoginCommand(user.Email, "wrong-password"), CancellationToken.None);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_LockedOutUser_ReturnsNull()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@hotel.com", UserName = "user@hotel.com" };

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, "correct-password")).ReturnsAsync(true);
        userManager.Setup(m => m.IsLockedOutAsync(user)).ReturnsAsync(true);

        var handler = new LoginCommandHandler(userManager.Object, Mock.Of<IJwtTokenService>(), Mock.Of<IRefreshTokenRepository>(), Mock.Of<IUnitOfWork>(), DefaultJwtSettings);
        var result = await handler.HandleAsync(new LoginCommand(user.Email, "correct-password"), CancellationToken.None);

        result.ShouldBeNull();
    }
}

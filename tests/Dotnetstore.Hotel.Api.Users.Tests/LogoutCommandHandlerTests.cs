using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.Logout;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class LogoutCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ActiveToken_RevokesAndReturnsTrue()
    {
        var token = RefreshToken.Create(Guid.NewGuid(), "hash", DateTimeOffset.UtcNow.AddDays(1));

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(token);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new LogoutCommandHandler(refreshTokenRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new LogoutCommand("raw-token"), CancellationToken.None);

        result.ShouldBeTrue();
        token.IsActive.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UnknownToken_ReturnsFalseWithoutSaving()
    {
        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new LogoutCommandHandler(refreshTokenRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new LogoutCommand("unknown-token"), CancellationToken.None);

        result.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

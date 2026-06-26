using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.RevokeUserTokens;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class RevokeUserTokensCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ActiveTokensExist_RevokesAllAndReturnsCount()
    {
        var userId = Guid.NewGuid();
        var tokenA = RefreshToken.Create(userId, "hash-a", DateTimeOffset.UtcNow.AddDays(1));
        var tokenB = RefreshToken.Create(userId, "hash-b", DateTimeOffset.UtcNow.AddDays(1));

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([tokenA, tokenB]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RevokeUserTokensCommandHandler(refreshTokenRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new RevokeUserTokensCommand(userId), CancellationToken.None);

        result.ShouldBe(2);
        tokenA.IsActive.ShouldBeFalse();
        tokenB.IsActive.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NoActiveTokens_ReturnsZeroWithoutSaving()
    {
        var userId = Guid.NewGuid();

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new RevokeUserTokensCommandHandler(refreshTokenRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new RevokeUserTokensCommand(userId), CancellationToken.None);

        result.ShouldBe(0);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

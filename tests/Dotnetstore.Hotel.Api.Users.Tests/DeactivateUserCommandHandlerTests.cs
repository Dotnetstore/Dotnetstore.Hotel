using Dotnetstore.Hotel.Api.Users.Domain;
using Dotnetstore.Hotel.Api.Users.Features.DeactivateUser;
using Dotnetstore.Hotel.Api.Users.Persistence;
using Microsoft.AspNetCore.Identity;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Users.Tests;

public class DeactivateUserCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ExistingUser_LocksOutAndRevokesActiveTokens()
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "user@hotel.com", UserName = "user" };
        var token = RefreshToken.Create(user.Id, "hash", DateTimeOffset.UtcNow.AddDays(1));

        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        userManager.Setup(m => m.SetLockoutEnabledAsync(user, true)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue)).ReturnsAsync(IdentityResult.Success);

        var refreshTokenRepository = new Mock<IRefreshTokenRepository>();
        refreshTokenRepository.Setup(r => r.GetActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync([token]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeactivateUserCommandHandler(userManager.Object, refreshTokenRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeactivateUserCommand(user.Id, Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeTrue();
        token.IsActive.ShouldBeFalse();
        userManager.Verify(m => m.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_SelfDeactivation_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var userManager = IdentityMocks.CreateUserManagerMock();

        var handler = new DeactivateUserCommandHandler(userManager.Object, Mock.Of<IRefreshTokenRepository>(), Mock.Of<IUnitOfWork>());
        var result = await handler.HandleAsync(new DeactivateUserCommand(userId, userId), CancellationToken.None);

        result.ShouldBeFalse();
        userManager.Verify(m => m.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsFalse()
    {
        var userManager = IdentityMocks.CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

        var handler = new DeactivateUserCommandHandler(userManager.Object, Mock.Of<IRefreshTokenRepository>(), Mock.Of<IUnitOfWork>());
        var result = await handler.HandleAsync(new DeactivateUserCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeFalse();
    }
}

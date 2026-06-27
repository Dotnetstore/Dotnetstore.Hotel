using Dotnetstore.Hotel.Api.Hotels.Features.CreateTag;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using TagEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class CreateTagCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidName_CreatesAndSaves()
    {
        var repository = new Mock<ITagRepository>();
        repository.Setup(r => r.ExistsByNameAsync("Hard", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateTagCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CreateTagCommand("Hard"), CancellationToken.None);

        result.Tag.ShouldNotBeNull();
        result.Tag.Name.ShouldBe("Hard");
        result.Errors.ShouldBeEmpty();
        repository.Verify(r => r.AddAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_BlankName_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<ITagRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateTagCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CreateTagCommand("   "), CancellationToken.None);

        result.Tag.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<ITagRepository>();
        repository.Setup(r => r.ExistsByNameAsync("Hard", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateTagCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CreateTagCommand("Hard"), CancellationToken.None);

        result.Tag.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

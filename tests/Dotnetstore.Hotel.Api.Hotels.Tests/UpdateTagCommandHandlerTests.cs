using Dotnetstore.Hotel.Api.Hotels.Features.UpdateTag;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using TagEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class UpdateTagCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_TagExists_UpdatesAndSaves()
    {
        var tag = TagEntity.Create(Guid.NewGuid(), "Hard");

        var repository = new Mock<ITagRepository>();
        repository.Setup(r => r.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);
        repository.Setup(r => r.ExistsByNameAsync("Soft", tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateTagCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new UpdateTagCommand(tag.Id, "Soft"), CancellationToken.None);

        result.Tag.ShouldNotBeNull();
        result.Tag.Name.ShouldBe("Soft");
        repository.Verify(r => r.Update(tag), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_TagDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<ITagRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TagEntity?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateTagCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new UpdateTagCommand(Guid.NewGuid(), "Soft"), CancellationToken.None);

        result.Tag.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

using Dotnetstore.Hotel.Api.Hotels.Features.DeleteTag;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using TagEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class DeleteTagCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_TagExistsAndUnused_RemovesAndSaves()
    {
        var tag = TagEntity.Create(Guid.NewGuid(), "Hard");

        var tagRepository = new Mock<ITagRepository>();
        tagRepository.Setup(r => r.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.CountUsingTagAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteTagCommandHandler(tagRepository.Object, roomRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteTagCommand(tag.Id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        tagRepository.Verify(r => r.Remove(tag), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_TagInUseByRooms_ReturnsErrorAndDoesNotSave()
    {
        var tag = TagEntity.Create(Guid.NewGuid(), "Hard");

        var tagRepository = new Mock<ITagRepository>();
        tagRepository.Setup(r => r.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.CountUsingTagAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteTagCommandHandler(tagRepository.Object, roomRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteTagCommand(tag.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        tagRepository.Verify(r => r.Remove(It.IsAny<TagEntity>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_TagDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var tagRepository = new Mock<ITagRepository>();
        tagRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TagEntity?)null);

        var roomRepository = new Mock<IRoomRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteTagCommandHandler(tagRepository.Object, roomRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteTagCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

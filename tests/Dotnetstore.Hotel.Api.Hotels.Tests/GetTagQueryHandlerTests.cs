using Dotnetstore.Hotel.Api.Hotels.Features.GetTag;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using TagEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class GetTagQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_TagExists_ReturnsMappedDto()
    {
        var tag = TagEntity.Create(Guid.NewGuid(), "55-inch");

        var repository = new Mock<ITagRepository>();
        repository.Setup(r => r.GetByIdAsync(tag.Id, It.IsAny<CancellationToken>())).ReturnsAsync(tag);

        var handler = new GetTagQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetTagQuery(tag.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("55-inch");
    }

    [Fact]
    public async Task HandleAsync_TagDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<ITagRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TagEntity?)null);

        var handler = new GetTagQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetTagQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}

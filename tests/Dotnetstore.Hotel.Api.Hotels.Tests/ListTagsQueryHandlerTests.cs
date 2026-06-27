using Dotnetstore.Hotel.Api.Hotels.Features.ListTags;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using TagEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Tag;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class ListTagsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsMappedDtoPerTag()
    {
        var hard = TagEntity.Create(Guid.NewGuid(), "Hard");
        var soft = TagEntity.Create(Guid.NewGuid(), "Soft");

        var repository = new Mock<ITagRepository>();
        repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([hard, soft]);

        var handler = new ListTagsQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new ListTagsQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Single(t => t.Id == hard.Id).Name.ShouldBe("Hard");
        result.Single(t => t.Id == soft.Id).Name.ShouldBe("Soft");
    }
}

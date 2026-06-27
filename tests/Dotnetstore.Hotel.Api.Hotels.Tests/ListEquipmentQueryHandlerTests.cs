using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.ListEquipment;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class ListEquipmentQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsMappedDtoPerEquipment()
    {
        var bed = Equipment.Create(Guid.NewGuid(), "Bed", null);
        var tv = Equipment.Create(Guid.NewGuid(), "TV", "55-inch flatscreen");

        var repository = new Mock<IEquipmentRepository>();
        repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([bed, tv]);

        var handler = new ListEquipmentQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new ListEquipmentQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Single(e => e.Id == bed.Id).Name.ShouldBe("Bed");
        result.Single(e => e.Id == tv.Id).Description.ShouldBe("55-inch flatscreen");
    }
}

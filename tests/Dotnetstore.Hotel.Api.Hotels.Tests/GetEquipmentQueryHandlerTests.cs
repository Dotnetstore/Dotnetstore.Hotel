using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.GetEquipment;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class GetEquipmentQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_EquipmentExists_ReturnsMappedDto()
    {
        var equipment = Equipment.Create(Guid.NewGuid(), "Phone", "Bedside landline");

        var repository = new Mock<IEquipmentRepository>();
        repository.Setup(r => r.GetByIdAsync(equipment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);

        var handler = new GetEquipmentQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetEquipmentQuery(equipment.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Phone");
        result.Description.ShouldBe("Bedside landline");
    }

    [Fact]
    public async Task HandleAsync_EquipmentDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<IEquipmentRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Equipment?)null);

        var handler = new GetEquipmentQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetEquipmentQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}

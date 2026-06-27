using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateEquipment;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class UpdateEquipmentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EquipmentExists_UpdatesAndSaves()
    {
        var equipment = Equipment.Create(Guid.NewGuid(), "Old Name", "Old description");

        var repository = new Mock<IEquipmentRepository>();
        repository.Setup(r => r.GetByIdAsync(equipment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateEquipmentCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new UpdateEquipmentCommand(equipment.Id, "New Name", "New description"), CancellationToken.None);

        result.Equipment.ShouldNotBeNull();
        result.Equipment.Name.ShouldBe("New Name");
        result.Equipment.Description.ShouldBe("New description");
        repository.Verify(r => r.Update(equipment), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EquipmentDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<IEquipmentRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Equipment?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateEquipmentCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new UpdateEquipmentCommand(Guid.NewGuid(), "Name", null), CancellationToken.None);

        result.Equipment.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

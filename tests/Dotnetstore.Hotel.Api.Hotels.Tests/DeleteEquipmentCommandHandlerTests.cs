using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteEquipment;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class DeleteEquipmentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EquipmentExists_RemovesAndSaves()
    {
        var equipment = Equipment.Create(Guid.NewGuid(), "Bathroom", null);

        var repository = new Mock<IEquipmentRepository>();
        repository.Setup(r => r.GetByIdAsync(equipment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteEquipmentCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteEquipmentCommand(equipment.Id), CancellationToken.None);

        result.ShouldBeTrue();
        repository.Verify(r => r.Remove(equipment), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EquipmentDoesNotExist_ReturnsFalseAndDoesNotSave()
    {
        var repository = new Mock<IEquipmentRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Equipment?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteEquipmentCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteEquipmentCommand(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteEquipment;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class DeleteEquipmentCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_EquipmentExistsAndUnused_RemovesAndSaves()
    {
        var equipment = Equipment.Create(Guid.NewGuid(), "Bathroom", null);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        equipmentRepository.Setup(r => r.GetByIdAsync(equipment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.CountUsingEquipmentAsync(equipment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteEquipmentCommandHandler(equipmentRepository.Object, roomRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteEquipmentCommand(equipment.Id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        equipmentRepository.Verify(r => r.Remove(equipment), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EquipmentInUseByRooms_ReturnsErrorAndDoesNotSave()
    {
        var equipment = Equipment.Create(Guid.NewGuid(), "TV", null);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        equipmentRepository.Setup(r => r.GetByIdAsync(equipment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(equipment);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.CountUsingEquipmentAsync(equipment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteEquipmentCommandHandler(equipmentRepository.Object, roomRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteEquipmentCommand(equipment.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        equipmentRepository.Verify(r => r.Remove(It.IsAny<Equipment>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EquipmentDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var equipmentRepository = new Mock<IEquipmentRepository>();
        equipmentRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Equipment?)null);

        var roomRepository = new Mock<IRoomRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteEquipmentCommandHandler(equipmentRepository.Object, roomRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteEquipmentCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

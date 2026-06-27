using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.UpdateRoom;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Moq;
using Shouldly;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class UpdateRoomCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RoomExists_UpdatesAndSaves()
    {
        var room = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        var tv = Equipment.Create(Guid.NewGuid(), "TV", null);
        var bigScreen = Tag.Create(Guid.NewGuid(), "55-inch");

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(room);
        roomRepository.Setup(r => r.ExistsByRoomNumberAsync("102", room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        equipmentRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([tv]);

        var tagRepository = new Mock<ITagRepository>();
        tagRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([bigScreen]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateRoomCommandHandler(roomRepository.Object, equipmentRepository.Object, tagRepository.Object, unitOfWork.Object);
        var command = new UpdateRoomCommand(room.Id, "102", 2, 4, "Suite", 299.00m, RoomStatuses.Maintenance, [new RoomEquipmentInput(tv.Id, [bigScreen.Id])]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Room.ShouldNotBeNull();
        result.Room.RoomNumber.ShouldBe("102");
        result.Room.Status.ShouldBe(RoomStatuses.Maintenance);
        var equipmentEntry = result.Room.Equipment.Single();
        equipmentEntry.Equipment.Id.ShouldBe(tv.Id);
        equipmentEntry.Tags.ShouldContain(t => t.Id == bigScreen.Id);
        roomRepository.Verify(r => r.Update(room), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RoomDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoomEntity?)null);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        var tagRepository = new Mock<ITagRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new UpdateRoomCommandHandler(roomRepository.Object, equipmentRepository.Object, tagRepository.Object, unitOfWork.Object);
        var command = new UpdateRoomCommand(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available, []);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Room.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.CreateRoom;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using Moq;
using Shouldly;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class CreateRoomCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommandWithEquipmentAndTags_CreatesAndSaves()
    {
        var tv = Equipment.Create(Guid.NewGuid(), "TV", null);
        var bigScreen = Tag.Create(Guid.NewGuid(), "55-inch");

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.ExistsByRoomNumberAsync("101", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        equipmentRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([tv]);

        var tagRepository = new Mock<ITagRepository>();
        tagRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([bigScreen]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateRoomCommandHandler(roomRepository.Object, equipmentRepository.Object, tagRepository.Object, unitOfWork.Object);
        var command = new CreateRoomCommand("101", 1, 2, "Double", 150.00m, RoomStatuses.Available, [new RoomEquipmentInput(tv.Id, [bigScreen.Id])]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Room.ShouldNotBeNull();
        result.Room.RoomNumber.ShouldBe("101");
        var equipmentEntry = result.Room.Equipment.Single();
        equipmentEntry.Equipment.Id.ShouldBe(tv.Id);
        equipmentEntry.Tags.ShouldContain(t => t.Id == bigScreen.Id);
        result.Errors.ShouldBeEmpty();
        roomRepository.Verify(r => r.AddAsync(It.IsAny<RoomEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_DuplicateRoomNumber_ReturnsErrorAndDoesNotSave()
    {
        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.ExistsByRoomNumberAsync("101", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        var tagRepository = new Mock<ITagRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateRoomCommandHandler(roomRepository.Object, equipmentRepository.Object, tagRepository.Object, unitOfWork.Object);
        var command = new CreateRoomCommand("101", 1, 2, "Double", 150.00m, RoomStatuses.Available, []);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Room.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UnknownEquipmentId_ReturnsErrorAndDoesNotSave()
    {
        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.ExistsByRoomNumberAsync("101", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        equipmentRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var tagRepository = new Mock<ITagRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateRoomCommandHandler(roomRepository.Object, equipmentRepository.Object, tagRepository.Object, unitOfWork.Object);
        var command = new CreateRoomCommand("101", 1, 2, "Double", 150.00m, RoomStatuses.Available, [new RoomEquipmentInput(Guid.NewGuid(), [])]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Room.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_UnknownTagId_ReturnsErrorAndDoesNotSave()
    {
        var tv = Equipment.Create(Guid.NewGuid(), "TV", null);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.ExistsByRoomNumberAsync("101", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var equipmentRepository = new Mock<IEquipmentRepository>();
        equipmentRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([tv]);

        var tagRepository = new Mock<ITagRepository>();
        tagRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateRoomCommandHandler(roomRepository.Object, equipmentRepository.Object, tagRepository.Object, unitOfWork.Object);
        var command = new CreateRoomCommand("101", 1, 2, "Double", 150.00m, RoomStatuses.Available, [new RoomEquipmentInput(tv.Id, [Guid.NewGuid()])]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Room.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_InvalidStatus_ReturnsErrorAndDoesNotSave()
    {
        var roomRepository = new Mock<IRoomRepository>();
        var equipmentRepository = new Mock<IEquipmentRepository>();
        var tagRepository = new Mock<ITagRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateRoomCommandHandler(roomRepository.Object, equipmentRepository.Object, tagRepository.Object, unitOfWork.Object);
        var command = new CreateRoomCommand("101", 1, 2, "Double", 150.00m, "NotAStatus", []);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Room.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

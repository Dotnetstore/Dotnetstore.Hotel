using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.DeleteRoom;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class DeleteRoomCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_RoomExistsAndUnused_RemovesAndSaves()
    {
        var room = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository.Setup(r => r.CountActiveBookingsForRoomAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteRoomCommandHandler(roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteRoomCommand(room.Id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        roomRepository.Verify(r => r.Remove(room), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_RoomHasActiveBookings_ReturnsErrorAndDoesNotSave()
    {
        var room = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository.Setup(r => r.CountActiveBookingsForRoomAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteRoomCommandHandler(roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteRoomCommand(room.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        roomRepository.Verify(r => r.Remove(It.IsAny<RoomEntity>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RoomDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoomEntity?)null);

        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new DeleteRoomCommandHandler(roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new DeleteRoomCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

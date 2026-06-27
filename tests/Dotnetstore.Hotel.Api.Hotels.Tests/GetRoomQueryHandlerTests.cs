using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.GetRoom;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class GetRoomQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_RoomExists_ReturnsMappedDto()
    {
        var room = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);

        var repository = new Mock<IRoomRepository>();
        repository.Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var handler = new GetRoomQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetRoomQuery(room.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.RoomNumber.ShouldBe("101");
        result.BedType.ShouldBe("Double");
    }

    [Fact]
    public async Task HandleAsync_RoomDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<IRoomRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoomEntity?)null);

        var handler = new GetRoomQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetRoomQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}

using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.ListRooms;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class ListRoomsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsMappedDtoPerRoom()
    {
        var single = RoomEntity.Create(Guid.NewGuid(), "101", 1, 1, "Single", 99.00m, RoomStatuses.Available);
        var suite = RoomEntity.Create(Guid.NewGuid(), "202", 2, 4, "Suite", 299.00m, RoomStatuses.Maintenance);

        var repository = new Mock<IRoomRepository>();
        repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([single, suite]);

        var handler = new ListRoomsQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new ListRoomsQuery(), CancellationToken.None);

        result.Count.ShouldBe(2);
        result.Single(r => r.Id == single.Id).RoomNumber.ShouldBe("101");
        result.Single(r => r.Id == suite.Id).Status.ShouldBe(RoomStatuses.Maintenance);
    }
}

using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.SearchRooms;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class SearchRoomsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_NoFilters_ReturnsAllRooms()
    {
        var roomA = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        var roomB = RoomEntity.Create(Guid.NewGuid(), "102", 1, 2, "Double", 150.00m, RoomStatuses.Available);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([roomA, roomB]);

        var bookingRepository = new Mock<IBookingRepository>();

        var handler = new SearchRoomsQueryHandler(roomRepository.Object, bookingRepository.Object);
        var result = await handler.HandleAsync(new SearchRoomsQuery(null, null, [], []), CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task HandleAsync_EquipmentFilter_ReturnsOnlyRoomsWithAllRequestedEquipment()
    {
        var tv = Equipment.Create(Guid.NewGuid(), "TV", null);
        var bed = Equipment.Create(Guid.NewGuid(), "Bed", null);

        var roomWithTv = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        roomWithTv.SetEquipment([(tv, (IReadOnlyCollection<Tag>)[])]);

        var roomWithoutTv = RoomEntity.Create(Guid.NewGuid(), "102", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        roomWithoutTv.SetEquipment([(bed, (IReadOnlyCollection<Tag>)[])]);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([roomWithTv, roomWithoutTv]);

        var bookingRepository = new Mock<IBookingRepository>();

        var handler = new SearchRoomsQueryHandler(roomRepository.Object, bookingRepository.Object);
        var result = await handler.HandleAsync(new SearchRoomsQuery(null, null, [tv.Id], []), CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Id.ShouldBe(roomWithTv.Id);
    }

    [Fact]
    public async Task HandleAsync_TagFilter_ReturnsOnlyRoomsWithAllRequestedTags()
    {
        var bed = Equipment.Create(Guid.NewGuid(), "Bed", null);
        var hard = Tag.Create(Guid.NewGuid(), "Hard");
        var soft = Tag.Create(Guid.NewGuid(), "Soft");

        var hardRoom = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        hardRoom.SetEquipment([(bed, (IReadOnlyCollection<Tag>)[hard])]);

        var softRoom = RoomEntity.Create(Guid.NewGuid(), "102", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        softRoom.SetEquipment([(bed, (IReadOnlyCollection<Tag>)[soft])]);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([hardRoom, softRoom]);

        var bookingRepository = new Mock<IBookingRepository>();

        var handler = new SearchRoomsQueryHandler(roomRepository.Object, bookingRepository.Object);
        var result = await handler.HandleAsync(new SearchRoomsQuery(null, null, [], [hard.Id]), CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Id.ShouldBe(hardRoom.Id);
    }

    [Fact]
    public async Task HandleAsync_DateFilter_ExcludesRoomsWithOverlappingBooking()
    {
        var roomA = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        var roomB = RoomEntity.Create(Guid.NewGuid(), "102", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var checkOut = checkIn.AddDays(2);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([roomA, roomB]);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository.Setup(r => r.HasOverlappingBookingAsync(roomA.Id, checkIn, checkOut, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        bookingRepository.Setup(r => r.HasOverlappingBookingAsync(roomB.Id, checkIn, checkOut, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var handler = new SearchRoomsQueryHandler(roomRepository.Object, bookingRepository.Object);
        var result = await handler.HandleAsync(new SearchRoomsQuery(checkIn, checkOut, [], []), CancellationToken.None);

        result.Count.ShouldBe(1);
        result.Single().Id.ShouldBe(roomB.Id);
    }
}

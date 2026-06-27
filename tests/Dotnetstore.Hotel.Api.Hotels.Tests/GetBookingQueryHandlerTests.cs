using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.GetBooking;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using BookingEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Booking;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class GetBookingQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_BookingExists_ReturnsMappedDto()
    {
        var customer = CustomerEntity.Create(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", new Address("S", "C", "Z", "Co"), "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);
        var room = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        var booking = BookingEntity.Create(Guid.NewGuid(), customer, DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), [room]);

        var repository = new Mock<IBookingRepository>();
        repository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var handler = new GetBookingQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetBookingQuery(booking.Id), CancellationToken.None);

        result.ShouldNotBeNull();
        result.Customer.FullName.ShouldBe("Alice");
        result.Rooms.ShouldContain(r => r.Id == room.Id);
        result.Status.ShouldBe(BookingStatuses.Reserved);
    }

    [Fact]
    public async Task HandleAsync_BookingDoesNotExist_ReturnsNull()
    {
        var repository = new Mock<IBookingRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BookingEntity?)null);

        var handler = new GetBookingQueryHandler(repository.Object);
        var result = await handler.HandleAsync(new GetBookingQuery(Guid.NewGuid()), CancellationToken.None);

        result.ShouldBeNull();
    }
}

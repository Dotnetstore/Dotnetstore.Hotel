using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.CheckOutBooking;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using BookingEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Booking;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class CheckOutBookingCommandHandlerTests
{
    private static CustomerEntity NewCustomer()
        => CustomerEntity.Create(Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", new Address("S", "C", "Z", "Co"), "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

    private static BookingEntity NewCheckedInBooking()
    {
        var room = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        var booking = BookingEntity.Create(Guid.NewGuid(), NewCustomer(), DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), [room]);
        booking.CheckIn();
        return booking;
    }

    [Fact]
    public async Task HandleAsync_CheckedInBooking_ChecksOutAndSaves()
    {
        var booking = NewCheckedInBooking();

        var repository = new Mock<IBookingRepository>();
        repository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CheckOutBookingCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CheckOutBookingCommand(booking.Id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        booking.Status.ShouldBe(BookingStatuses.CheckedOut);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReservedBookingNotCheckedIn_ReturnsErrorAndDoesNotSave()
    {
        var room = RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);
        var booking = BookingEntity.Create(Guid.NewGuid(), NewCustomer(), DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), [room]);

        var repository = new Mock<IBookingRepository>();
        repository.Setup(r => r.GetByIdAsync(booking.Id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CheckOutBookingCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CheckOutBookingCommand(booking.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BookingDoesNotExist_ReturnsErrorAndDoesNotSave()
    {
        var repository = new Mock<IBookingRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((BookingEntity?)null);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CheckOutBookingCommandHandler(repository.Object, unitOfWork.Object);
        var result = await handler.HandleAsync(new CheckOutBookingCommand(Guid.NewGuid()), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

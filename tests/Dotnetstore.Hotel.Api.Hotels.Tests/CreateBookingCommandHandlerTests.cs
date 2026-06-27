using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Features.CreateBooking;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Moq;
using Shouldly;
using CustomerEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Customer;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Tests;

public class CreateBookingCommandHandlerTests
{
    private static CustomerEntity NewCustomer() => CustomerEntity.Create(
        Guid.NewGuid(), "Alice", IdentificationTypes.Passport, "P1", new Address("S", "C", "Z", "Co"), "111", "alice@x.com", new DateOnly(1990, 1, 1), "Swedish", null);

    private static RoomEntity NewRoom() => RoomEntity.Create(Guid.NewGuid(), "101", 1, 2, "Double", 150.00m, RoomStatuses.Available);

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesAndSaves()
    {
        var customer = NewCustomer();
        var room = NewRoom();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var checkOut = checkIn.AddDays(2);

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository.Setup(r => r.HasOverlappingBookingAsync(room.Id, checkIn, checkOut, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateBookingCommandHandler(customerRepository.Object, roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var command = new CreateBookingCommand(customer.Id, checkIn, checkOut, [room.Id]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Booking.ShouldNotBeNull();
        result.Booking.Status.ShouldBe(BookingStatuses.Reserved);
        result.Booking.Rooms.ShouldContain(r => r.Id == room.Id);
        result.Errors.ShouldBeEmpty();
        bookingRepository.Verify(r => r.AddAsync(It.IsAny<Domain.Booking>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_CheckOutNotAfterCheckIn_ReturnsErrorAndDoesNotSave()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        var roomRepository = new Mock<IRoomRepository>();
        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateBookingCommandHandler(customerRepository.Object, roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = new CreateBookingCommand(Guid.NewGuid(), checkIn, checkIn, [Guid.NewGuid()]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Booking.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CheckInInThePast_ReturnsErrorAndDoesNotSave()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        var roomRepository = new Mock<IRoomRepository>();
        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateBookingCommandHandler(customerRepository.Object, roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var command = new CreateBookingCommand(Guid.NewGuid(), checkIn, checkIn.AddDays(1), [Guid.NewGuid()]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Booking.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoRoomIds_ReturnsErrorAndDoesNotSave()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        var roomRepository = new Mock<IRoomRepository>();
        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateBookingCommandHandler(customerRepository.Object, roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = new CreateBookingCommand(Guid.NewGuid(), checkIn, checkIn.AddDays(1), []);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Booking.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_CustomerNotFound_ReturnsErrorAndDoesNotSave()
    {
        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((CustomerEntity?)null);

        var roomRepository = new Mock<IRoomRepository>();
        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateBookingCommandHandler(customerRepository.Object, roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = new CreateBookingCommand(Guid.NewGuid(), checkIn, checkIn.AddDays(1), [Guid.NewGuid()]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Booking.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RoomNotFound_ReturnsErrorAndDoesNotSave()
    {
        var customer = NewCustomer();

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((RoomEntity?)null);

        var bookingRepository = new Mock<IBookingRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateBookingCommandHandler(customerRepository.Object, roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var command = new CreateBookingCommand(customer.Id, checkIn, checkIn.AddDays(1), [Guid.NewGuid()]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Booking.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_RoomHasOverlappingBooking_ReturnsErrorAndDoesNotSave()
    {
        var customer = NewCustomer();
        var room = NewRoom();
        var checkIn = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var checkOut = checkIn.AddDays(2);

        var customerRepository = new Mock<ICustomerRepository>();
        customerRepository.Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>())).ReturnsAsync(customer);

        var roomRepository = new Mock<IRoomRepository>();
        roomRepository.Setup(r => r.GetByIdAsync(room.Id, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        var bookingRepository = new Mock<IBookingRepository>();
        bookingRepository.Setup(r => r.HasOverlappingBookingAsync(room.Id, checkIn, checkOut, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateBookingCommandHandler(customerRepository.Object, roomRepository.Object, bookingRepository.Object, unitOfWork.Object);
        var command = new CreateBookingCommand(customer.Id, checkIn, checkOut, [room.Id]);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Booking.ShouldBeNull();
        result.Errors.ShouldNotBeEmpty();
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

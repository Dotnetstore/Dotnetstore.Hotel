using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;
using BookingEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateBooking;

public class CreateBookingCommandHandler(
    ICustomerRepository customerRepository,
    IRoomRepository roomRepository,
    IBookingRepository bookingRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBookingCommand, CreateBookingResponse>
{
    public async Task<CreateBookingResponse> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        if (command.CheckOutDate <= command.CheckInDate)
        {
            return new CreateBookingResponse(null, ["Check-out date must be after check-in date."]);
        }

        if (command.CheckInDate < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return new CreateBookingResponse(null, ["Check-in date cannot be in the past."]);
        }

        if (command.RoomIds.Count == 0)
        {
            return new CreateBookingResponse(null, ["At least one room is required."]);
        }

        var customer = await customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);
        if (customer is null)
        {
            return new CreateBookingResponse(null, ["Customer not found."]);
        }

        var rooms = new List<Domain.Room>();
        foreach (var roomId in command.RoomIds.Distinct())
        {
            var room = await roomRepository.GetByIdAsync(roomId, cancellationToken);
            if (room is null)
            {
                return new CreateBookingResponse(null, [$"Room '{roomId}' not found."]);
            }

            if (await bookingRepository.HasOverlappingBookingAsync(roomId, command.CheckInDate, command.CheckOutDate, excludingBookingId: null, cancellationToken))
            {
                return new CreateBookingResponse(null, [$"Room '{room.RoomNumber}' is not available for the requested dates."]);
            }

            rooms.Add(room);
        }

        var booking = BookingEntity.Create(Guid.NewGuid(), customer, command.CheckInDate, command.CheckOutDate, rooms);
        await bookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateBookingResponse(BookingDtoMapper.ToDto(booking), []);
    }
}

using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateBooking;

public record CreateBookingCommand(
    Guid CustomerId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    IReadOnlyCollection<Guid> RoomIds) : ICommand<CreateBookingResponse>;

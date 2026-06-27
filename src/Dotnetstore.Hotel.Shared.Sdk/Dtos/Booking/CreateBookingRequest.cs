namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

public record CreateBookingRequest(
    Guid CustomerId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    IReadOnlyCollection<Guid> RoomIds);

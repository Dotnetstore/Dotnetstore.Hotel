using Dotnetstore.Hotel.Shared.Sdk.Dtos.Customer;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

public record BookingDto(
    Guid Id,
    CustomerDto Customer,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string Status,
    IReadOnlyList<RoomDto> Rooms);

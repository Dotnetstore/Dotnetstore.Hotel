using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;
using BookingEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features;

internal static class BookingDtoMapper
{
    public static BookingDto ToDto(BookingEntity booking) => new(
        booking.Id,
        CustomerDtoMapper.ToDto(booking.Customer),
        booking.CheckInDate,
        booking.CheckOutDate,
        booking.Status,
        booking.Rooms.Select(RoomDtoMapper.ToDto).ToList());
}

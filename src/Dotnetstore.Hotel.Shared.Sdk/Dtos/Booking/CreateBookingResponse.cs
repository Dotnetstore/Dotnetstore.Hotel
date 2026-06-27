namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

public record CreateBookingResponse(BookingDto? Booking, IReadOnlyList<string> Errors);

namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

public record BookingActionResponse(bool Succeeded, IReadOnlyList<string> Errors);

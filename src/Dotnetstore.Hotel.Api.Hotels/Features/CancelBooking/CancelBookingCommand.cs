using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CancelBooking;

public record CancelBookingCommand(Guid Id) : ICommand<BookingActionResponse>;

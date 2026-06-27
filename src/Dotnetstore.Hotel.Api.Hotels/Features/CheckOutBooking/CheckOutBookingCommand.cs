using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CheckOutBooking;

public record CheckOutBookingCommand(Guid Id) : ICommand<BookingActionResponse>;

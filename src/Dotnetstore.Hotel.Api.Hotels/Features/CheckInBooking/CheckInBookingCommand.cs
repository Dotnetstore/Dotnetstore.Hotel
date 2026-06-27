using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CheckInBooking;

public record CheckInBookingCommand(Guid Id) : ICommand<BookingActionResponse>;

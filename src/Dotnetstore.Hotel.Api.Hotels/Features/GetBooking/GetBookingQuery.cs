using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetBooking;

public record GetBookingQuery(Guid Id) : IQuery<BookingDto?>;

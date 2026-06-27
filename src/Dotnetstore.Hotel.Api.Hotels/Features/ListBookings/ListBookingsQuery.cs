using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListBookings;

public record ListBookingsQuery(Guid? CustomerId) : IQuery<IReadOnlyList<BookingDto>>;

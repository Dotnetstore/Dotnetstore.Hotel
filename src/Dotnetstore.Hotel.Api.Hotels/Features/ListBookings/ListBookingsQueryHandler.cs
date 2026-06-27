using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListBookings;

public class ListBookingsQueryHandler(IBookingRepository bookingRepository) : IQueryHandler<ListBookingsQuery, IReadOnlyList<BookingDto>>
{
    public async Task<IReadOnlyList<BookingDto>> HandleAsync(ListBookingsQuery query, CancellationToken cancellationToken)
    {
        var bookings = await bookingRepository.GetAllAsync(query.CustomerId, cancellationToken);
        return bookings.Select(BookingDtoMapper.ToDto).ToList();
    }
}

using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetBooking;

public class GetBookingQueryHandler(IBookingRepository bookingRepository) : IQueryHandler<GetBookingQuery, BookingDto?>
{
    public async Task<BookingDto?> HandleAsync(GetBookingQuery query, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(query.Id, cancellationToken);
        return booking is null ? null : BookingDtoMapper.ToDto(booking);
    }
}

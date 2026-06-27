using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CancelBooking;

public class CancelBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CancelBookingCommand, BookingActionResponse>
{
    public async Task<BookingActionResponse> HandleAsync(CancelBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(command.Id, cancellationToken);
        if (booking is null)
        {
            return new BookingActionResponse(false, ["Booking not found."]);
        }

        if (!booking.Cancel())
        {
            return new BookingActionResponse(false, [$"Booking with status '{booking.Status}' cannot be cancelled."]);
        }

        bookingRepository.Update(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new BookingActionResponse(true, []);
    }
}

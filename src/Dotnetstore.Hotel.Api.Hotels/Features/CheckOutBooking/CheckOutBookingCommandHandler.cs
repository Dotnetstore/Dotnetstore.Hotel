using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CheckOutBooking;

public class CheckOutBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CheckOutBookingCommand, BookingActionResponse>
{
    public async Task<BookingActionResponse> HandleAsync(CheckOutBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(command.Id, cancellationToken);
        if (booking is null)
        {
            return new BookingActionResponse(false, ["Booking not found."]);
        }

        if (!booking.CheckOut())
        {
            return new BookingActionResponse(false, [$"Booking with status '{booking.Status}' cannot be checked out."]);
        }

        bookingRepository.Update(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new BookingActionResponse(true, []);
    }
}

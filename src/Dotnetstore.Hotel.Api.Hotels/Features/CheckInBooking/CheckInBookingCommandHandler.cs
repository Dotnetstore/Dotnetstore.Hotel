using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Booking;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CheckInBooking;

public class CheckInBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CheckInBookingCommand, BookingActionResponse>
{
    public async Task<BookingActionResponse> HandleAsync(CheckInBookingCommand command, CancellationToken cancellationToken)
    {
        var booking = await bookingRepository.GetByIdAsync(command.Id, cancellationToken);
        if (booking is null)
        {
            return new BookingActionResponse(false, ["Booking not found."]);
        }

        if (!booking.CheckIn())
        {
            return new BookingActionResponse(false, [$"Booking with status '{booking.Status}' cannot be checked in."]);
        }

        bookingRepository.Update(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new BookingActionResponse(true, []);
    }
}

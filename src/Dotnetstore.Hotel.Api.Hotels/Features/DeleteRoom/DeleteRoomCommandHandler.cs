using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteRoom;

public class DeleteRoomCommandHandler(IRoomRepository roomRepository, IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteRoomCommand, DeleteRoomResponse>
{
    public async Task<DeleteRoomResponse> HandleAsync(DeleteRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(command.Id, cancellationToken);
        if (room is null)
        {
            return new DeleteRoomResponse(false, ["Room not found."]);
        }

        var activeBookings = await bookingRepository.CountActiveBookingsForRoomAsync(command.Id, cancellationToken);
        if (activeBookings > 0)
        {
            return new DeleteRoomResponse(false, [$"{activeBookings} active booking(s) still use this room. Cancel them first."]);
        }

        roomRepository.Remove(room);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeleteRoomResponse(true, []);
    }
}

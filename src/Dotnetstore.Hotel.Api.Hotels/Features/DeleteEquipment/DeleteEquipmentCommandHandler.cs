using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteEquipment;

public class DeleteEquipmentCommandHandler(IEquipmentRepository equipmentRepository, IRoomRepository roomRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteEquipmentCommand, DeleteEquipmentResponse>
{
    public async Task<DeleteEquipmentResponse> HandleAsync(DeleteEquipmentCommand command, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetByIdAsync(command.Id, cancellationToken);
        if (equipment is null)
        {
            return new DeleteEquipmentResponse(false, ["Equipment not found."]);
        }

        var roomsUsingEquipment = await roomRepository.CountUsingEquipmentAsync(command.Id, cancellationToken);
        if (roomsUsingEquipment > 0)
        {
            return new DeleteEquipmentResponse(false, [$"{roomsUsingEquipment} room(s) still use this equipment. Remove it from those rooms first."]);
        }

        equipmentRepository.Remove(equipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new DeleteEquipmentResponse(true, []);
    }
}

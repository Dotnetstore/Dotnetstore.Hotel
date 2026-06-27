using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteEquipment;

public class DeleteEquipmentCommandHandler(IEquipmentRepository equipmentRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteEquipmentCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteEquipmentCommand command, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetByIdAsync(command.Id, cancellationToken);
        if (equipment is null)
        {
            return false;
        }

        equipmentRepository.Remove(equipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

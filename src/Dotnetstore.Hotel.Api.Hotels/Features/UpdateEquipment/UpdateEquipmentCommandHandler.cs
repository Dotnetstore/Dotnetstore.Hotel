using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateEquipment;

public class UpdateEquipmentCommandHandler(IEquipmentRepository equipmentRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEquipmentCommand, UpdateEquipmentResponse>
{
    public async Task<UpdateEquipmentResponse> HandleAsync(UpdateEquipmentCommand command, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetByIdAsync(command.Id, cancellationToken);
        if (equipment is null)
        {
            return new UpdateEquipmentResponse(null, ["Equipment not found."]);
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new UpdateEquipmentResponse(null, ["Equipment name is required."]);
        }

        equipment.UpdateDetails(command.Name, command.Description);
        equipmentRepository.Update(equipment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateEquipmentResponse(EquipmentDtoMapper.ToDto(equipment), []);
    }
}

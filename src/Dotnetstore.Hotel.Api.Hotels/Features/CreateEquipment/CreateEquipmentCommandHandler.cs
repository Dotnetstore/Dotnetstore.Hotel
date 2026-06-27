using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateEquipment;

public class CreateEquipmentCommandHandler(IEquipmentRepository equipmentRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEquipmentCommand, CreateEquipmentResponse>
{
    public async Task<CreateEquipmentResponse> HandleAsync(CreateEquipmentCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new CreateEquipmentResponse(null, ["Equipment name is required."]);
        }

        var equipment = Equipment.Create(Guid.NewGuid(), command.Name, command.Description);
        await equipmentRepository.AddAsync(equipment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateEquipmentResponse(EquipmentDtoMapper.ToDto(equipment), []);
    }
}

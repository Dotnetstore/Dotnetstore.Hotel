using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetEquipment;

public class GetEquipmentQueryHandler(IEquipmentRepository equipmentRepository) : IQueryHandler<GetEquipmentQuery, EquipmentDto?>
{
    public async Task<EquipmentDto?> HandleAsync(GetEquipmentQuery query, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetByIdAsync(query.Id, cancellationToken);
        return equipment is null ? null : EquipmentDtoMapper.ToDto(equipment);
    }
}

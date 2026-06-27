using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListEquipment;

public class ListEquipmentQueryHandler(IEquipmentRepository equipmentRepository) : IQueryHandler<ListEquipmentQuery, IReadOnlyList<EquipmentDto>>
{
    public async Task<IReadOnlyList<EquipmentDto>> HandleAsync(ListEquipmentQuery query, CancellationToken cancellationToken)
    {
        var equipment = await equipmentRepository.GetAllAsync(cancellationToken);
        return equipment.Select(EquipmentDtoMapper.ToDto).ToList();
    }
}

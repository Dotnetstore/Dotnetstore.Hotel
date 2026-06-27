using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features;

/// <summary>
/// Shared "build an EquipmentDto for this equipment" mapping, used by every Equipment feature.
/// Synchronous (unlike UserDtoMapper/RoleDtoMapper in Api.Users) since there are no async lookups involved.
/// </summary>
internal static class EquipmentDtoMapper
{
    public static EquipmentDto ToDto(Equipment equipment) => new(equipment.Id, equipment.Name, equipment.Description);
}

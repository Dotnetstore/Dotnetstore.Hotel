namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

public record UpdateEquipmentResponse(EquipmentDto? Equipment, IReadOnlyList<string> Errors);

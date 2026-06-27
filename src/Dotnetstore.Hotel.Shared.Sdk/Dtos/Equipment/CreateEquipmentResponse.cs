namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

public record CreateEquipmentResponse(EquipmentDto? Equipment, IReadOnlyList<string> Errors);

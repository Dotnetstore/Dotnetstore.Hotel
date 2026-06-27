namespace Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

public record DeleteEquipmentResponse(bool Succeeded, IReadOnlyList<string> Errors);

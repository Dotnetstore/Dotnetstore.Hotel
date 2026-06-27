using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateEquipment;

public record UpdateEquipmentCommand(Guid Id, string Name, string? Description) : ICommand<UpdateEquipmentResponse>;

using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateEquipment;

public record CreateEquipmentCommand(string Name, string? Description) : ICommand<CreateEquipmentResponse>;

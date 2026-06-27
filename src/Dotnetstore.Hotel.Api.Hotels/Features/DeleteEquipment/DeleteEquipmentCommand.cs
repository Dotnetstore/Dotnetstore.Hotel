using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteEquipment;

public record DeleteEquipmentCommand(Guid Id) : ICommand<DeleteEquipmentResponse>;

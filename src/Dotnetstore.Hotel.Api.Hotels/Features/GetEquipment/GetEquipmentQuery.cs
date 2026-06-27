using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.GetEquipment;

public record GetEquipmentQuery(Guid Id) : IQuery<EquipmentDto?>;

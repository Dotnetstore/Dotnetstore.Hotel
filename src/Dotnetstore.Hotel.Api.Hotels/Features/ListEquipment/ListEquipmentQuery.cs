using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Equipment;

namespace Dotnetstore.Hotel.Api.Hotels.Features.ListEquipment;

public record ListEquipmentQuery : IQuery<IReadOnlyList<EquipmentDto>>;

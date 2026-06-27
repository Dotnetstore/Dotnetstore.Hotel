using Dotnetstore.Hotel.Shared.Cqrs;

namespace Dotnetstore.Hotel.Api.Hotels.Features.DeleteEquipment;

public record DeleteEquipmentCommand(Guid Id) : ICommand<bool>;

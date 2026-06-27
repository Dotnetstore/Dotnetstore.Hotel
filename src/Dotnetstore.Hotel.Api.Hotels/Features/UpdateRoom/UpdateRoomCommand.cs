using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateRoom;

public record UpdateRoomCommand(
    Guid Id,
    string RoomNumber,
    int Floor,
    int Capacity,
    string BedType,
    decimal PricePerNight,
    string Status,
    IReadOnlyCollection<RoomEquipmentInput> Equipment) : ICommand<UpdateRoomResponse>;

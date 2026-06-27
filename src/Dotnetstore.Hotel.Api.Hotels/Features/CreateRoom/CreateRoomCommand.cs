using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateRoom;

public record CreateRoomCommand(
    string RoomNumber,
    int Floor,
    int Capacity,
    string BedType,
    decimal PricePerNight,
    string Status,
    IReadOnlyCollection<RoomEquipmentInput> Equipment) : ICommand<CreateRoomResponse>;

using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.UpdateRoom;

public class UpdateRoomCommandHandler(
    IRoomRepository roomRepository,
    IEquipmentRepository equipmentRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateRoomCommand, UpdateRoomResponse>
{
    public async Task<UpdateRoomResponse> HandleAsync(UpdateRoomCommand command, CancellationToken cancellationToken)
    {
        var room = await roomRepository.GetByIdAsync(command.Id, cancellationToken);
        if (room is null)
        {
            return new UpdateRoomResponse(null, ["Room not found."]);
        }

        if (string.IsNullOrWhiteSpace(command.RoomNumber))
        {
            return new UpdateRoomResponse(null, ["Room number is required."]);
        }

        if (command.Capacity <= 0)
        {
            return new UpdateRoomResponse(null, ["Capacity must be greater than zero."]);
        }

        if (command.PricePerNight < 0)
        {
            return new UpdateRoomResponse(null, ["Price per night cannot be negative."]);
        }

        if (!RoomStatuses.IsValid(command.Status))
        {
            return new UpdateRoomResponse(null, [$"Status must be one of: {string.Join(", ", RoomStatuses.All)}."]);
        }

        if (await roomRepository.ExistsByRoomNumberAsync(command.RoomNumber, command.Id, cancellationToken))
        {
            return new UpdateRoomResponse(null, [$"Room number '{command.RoomNumber}' is already in use."]);
        }

        var equipmentIds = command.Equipment.Select(e => e.EquipmentId).Distinct().ToList();
        var equipment = await equipmentRepository.GetByIdsAsync(equipmentIds, cancellationToken);
        var missingEquipmentIds = equipmentIds.Except(equipment.Select(e => e.Id)).ToList();
        if (missingEquipmentIds.Count > 0)
        {
            return new UpdateRoomResponse(null, missingEquipmentIds.Select(id => $"Equipment '{id}' not found.").ToList());
        }

        var tagIds = command.Equipment.SelectMany(e => e.TagIds).Distinct().ToList();
        var tags = await tagRepository.GetByIdsAsync(tagIds, cancellationToken);
        var missingTagIds = tagIds.Except(tags.Select(t => t.Id)).ToList();
        if (missingTagIds.Count > 0)
        {
            return new UpdateRoomResponse(null, missingTagIds.Select(id => $"Tag '{id}' not found.").ToList());
        }

        var equipmentById = equipment.ToDictionary(e => e.Id);
        var tagsById = tags.ToDictionary(t => t.Id);
        var items = command.Equipment
            .Select(item => (Equipment: equipmentById[item.EquipmentId], Tags: (IReadOnlyCollection<Tag>)item.TagIds.Select(id => tagsById[id]).ToList()))
            .ToList();

        room.UpdateDetails(command.RoomNumber, command.Floor, command.Capacity, command.BedType, command.PricePerNight, command.Status);
        room.SetEquipment(items);
        roomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateRoomResponse(RoomDtoMapper.ToDto(room), []);
    }
}

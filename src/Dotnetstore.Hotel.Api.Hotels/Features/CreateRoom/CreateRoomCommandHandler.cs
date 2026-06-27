using Dotnetstore.Hotel.Api.Hotels.Domain;
using Dotnetstore.Hotel.Api.Hotels.Persistence;
using Dotnetstore.Hotel.Shared.Cqrs;
using Dotnetstore.Hotel.Shared.Sdk.Dtos.Room;
using RoomEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Room;

namespace Dotnetstore.Hotel.Api.Hotels.Features.CreateRoom;

public class CreateRoomCommandHandler(
    IRoomRepository roomRepository,
    IEquipmentRepository equipmentRepository,
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoomCommand, CreateRoomResponse>
{
    public async Task<CreateRoomResponse> HandleAsync(CreateRoomCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RoomNumber))
        {
            return new CreateRoomResponse(null, ["Room number is required."]);
        }

        if (command.Capacity <= 0)
        {
            return new CreateRoomResponse(null, ["Capacity must be greater than zero."]);
        }

        if (command.PricePerNight < 0)
        {
            return new CreateRoomResponse(null, ["Price per night cannot be negative."]);
        }

        if (!RoomStatuses.IsValid(command.Status))
        {
            return new CreateRoomResponse(null, [$"Status must be one of: {string.Join(", ", RoomStatuses.All)}."]);
        }

        if (await roomRepository.ExistsByRoomNumberAsync(command.RoomNumber, excludingId: null, cancellationToken))
        {
            return new CreateRoomResponse(null, [$"Room number '{command.RoomNumber}' is already in use."]);
        }

        var equipmentIds = command.Equipment.Select(e => e.EquipmentId).Distinct().ToList();
        var equipment = await equipmentRepository.GetByIdsAsync(equipmentIds, cancellationToken);
        var missingEquipmentIds = equipmentIds.Except(equipment.Select(e => e.Id)).ToList();
        if (missingEquipmentIds.Count > 0)
        {
            return new CreateRoomResponse(null, missingEquipmentIds.Select(id => $"Equipment '{id}' not found.").ToList());
        }

        var tagIds = command.Equipment.SelectMany(e => e.TagIds).Distinct().ToList();
        var tags = await tagRepository.GetByIdsAsync(tagIds, cancellationToken);
        var missingTagIds = tagIds.Except(tags.Select(t => t.Id)).ToList();
        if (missingTagIds.Count > 0)
        {
            return new CreateRoomResponse(null, missingTagIds.Select(id => $"Tag '{id}' not found.").ToList());
        }

        var equipmentById = equipment.ToDictionary(e => e.Id);
        var tagsById = tags.ToDictionary(t => t.Id);
        var items = command.Equipment
            .Select(item => (Equipment: equipmentById[item.EquipmentId], Tags: (IReadOnlyCollection<Tag>)item.TagIds.Select(id => tagsById[id]).ToList()))
            .ToList();

        var room = RoomEntity.Create(Guid.NewGuid(), command.RoomNumber, command.Floor, command.Capacity, command.BedType, command.PricePerNight, command.Status);
        room.SetEquipment(items);

        await roomRepository.AddAsync(room, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateRoomResponse(RoomDtoMapper.ToDto(room), []);
    }
}

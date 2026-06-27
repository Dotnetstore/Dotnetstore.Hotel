using Dotnetstore.Hotel.Api.Hotels.Domain;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Room>> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(Room room, CancellationToken cancellationToken);

    void Update(Room room);

    void Remove(Room room);

    Task<bool> ExistsByRoomNumberAsync(string roomNumber, Guid? excludingId, CancellationToken cancellationToken);

    Task<int> CountUsingEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken);

    Task<int> CountUsingTagAsync(Guid tagId, CancellationToken cancellationToken);
}

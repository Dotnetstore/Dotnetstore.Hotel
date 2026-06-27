using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

internal sealed class RoomRepository(HotelDbContext dbContext) : IRoomRepository
{
    public Task<Room?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Rooms
            .Include(r => r.Equipment).ThenInclude(re => re.Equipment)
            .Include(r => r.Equipment).ThenInclude(re => re.Tags)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<List<Room>> GetAllAsync(CancellationToken cancellationToken)
        => dbContext.Rooms
            .Include(r => r.Equipment).ThenInclude(re => re.Equipment)
            .Include(r => r.Equipment).ThenInclude(re => re.Tags)
            .ToListAsync(cancellationToken);

    public Task AddAsync(Room room, CancellationToken cancellationToken)
    {
        dbContext.Rooms.Add(room);
        return Task.CompletedTask;
    }

    public void Update(Room room) => dbContext.Rooms.Update(room);

    public void Remove(Room room) => dbContext.Rooms.Remove(room);

    public Task<bool> ExistsByRoomNumberAsync(string roomNumber, Guid? excludingId, CancellationToken cancellationToken)
        => dbContext.Rooms.AnyAsync(r => r.RoomNumber == roomNumber && r.Id != excludingId, cancellationToken);

    public Task<int> CountUsingEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken)
        => dbContext.Rooms.CountAsync(r => r.Equipment.Any(re => re.EquipmentId == equipmentId), cancellationToken);

    public Task<int> CountUsingTagAsync(Guid tagId, CancellationToken cancellationToken)
        => dbContext.Rooms.CountAsync(r => r.Equipment.Any(re => re.Tags.Any(t => t.Id == tagId)), cancellationToken);
}

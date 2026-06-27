using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

internal sealed class EquipmentRepository(HotelDbContext dbContext) : IEquipmentRepository
{
    public Task<Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<List<Equipment>> GetAllAsync(CancellationToken cancellationToken)
        => dbContext.Equipment.ToListAsync(cancellationToken);

    public Task<List<Equipment>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => dbContext.Equipment.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);

    public Task AddAsync(Equipment equipment, CancellationToken cancellationToken)
    {
        dbContext.Equipment.Add(equipment);
        return Task.CompletedTask;
    }

    public void Update(Equipment equipment) => dbContext.Equipment.Update(equipment);

    public void Remove(Equipment equipment) => dbContext.Equipment.Remove(equipment);
}

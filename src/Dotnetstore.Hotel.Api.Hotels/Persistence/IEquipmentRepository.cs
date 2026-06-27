using Dotnetstore.Hotel.Api.Hotels.Domain;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public interface IEquipmentRepository
{
    Task<Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Equipment>> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(Equipment equipment, CancellationToken cancellationToken);

    void Update(Equipment equipment);

    void Remove(Equipment equipment);
}

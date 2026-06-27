using Dotnetstore.Hotel.Api.Hotels.Domain;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken);

    Task<List<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task AddAsync(Tag tag, CancellationToken cancellationToken);

    void Update(Tag tag);

    void Remove(Tag tag);

    Task<bool> ExistsByNameAsync(string name, Guid? excludingId, CancellationToken cancellationToken);
}

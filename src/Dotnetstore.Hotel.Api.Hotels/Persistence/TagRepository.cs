using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

internal sealed class TagRepository(HotelDbContext dbContext) : ITagRepository
{
    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<List<Tag>> GetAllAsync(CancellationToken cancellationToken)
        => dbContext.Tags.ToListAsync(cancellationToken);

    public Task<List<Tag>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => dbContext.Tags.Where(t => ids.Contains(t.Id)).ToListAsync(cancellationToken);

    public Task AddAsync(Tag tag, CancellationToken cancellationToken)
    {
        dbContext.Tags.Add(tag);
        return Task.CompletedTask;
    }

    public void Update(Tag tag) => dbContext.Tags.Update(tag);

    public void Remove(Tag tag) => dbContext.Tags.Remove(tag);

    public Task<bool> ExistsByNameAsync(string name, Guid? excludingId, CancellationToken cancellationToken)
        => dbContext.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower() && t.Id != excludingId, cancellationToken);
}

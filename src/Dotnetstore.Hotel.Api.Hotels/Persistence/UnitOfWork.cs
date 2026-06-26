namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

internal sealed class UnitOfWork(HotelDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}

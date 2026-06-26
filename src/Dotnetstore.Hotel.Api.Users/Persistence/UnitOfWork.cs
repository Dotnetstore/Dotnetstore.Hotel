namespace Dotnetstore.Hotel.Api.Users.Persistence;

internal sealed class UnitOfWork(UsersDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

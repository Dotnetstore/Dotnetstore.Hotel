using Dotnetstore.Hotel.Api.Hotels.Domain;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Customer>> SearchAsync(string? searchTerm, CancellationToken cancellationToken);

    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    void Update(Customer customer);

    void Remove(Customer customer);

    Task<bool> ExistsByIdentificationNumberAsync(string identificationNumber, Guid? excludingId, CancellationToken cancellationToken);
}

using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

internal sealed class CustomerRepository(HotelDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<List<Customer>> SearchAsync(string? searchTerm, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return dbContext.Customers.ToListAsync(cancellationToken);
        }

        var term = searchTerm.Trim();
        var matchesNumber = int.TryParse(term, out var customerNumber);

        return dbContext.Customers
            .Where(c =>
                (matchesNumber && c.CustomerNumber == customerNumber) ||
                c.IdentificationNumber.ToLower().Contains(term.ToLower()) ||
                c.FullName.ToLower().Contains(term.ToLower()))
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        dbContext.Customers.Add(customer);
        return Task.CompletedTask;
    }

    public void Update(Customer customer) => dbContext.Customers.Update(customer);

    public void Remove(Customer customer) => dbContext.Customers.Remove(customer);

    public Task<bool> ExistsByIdentificationNumberAsync(string identificationNumber, Guid? excludingId, CancellationToken cancellationToken)
        => dbContext.Customers.AnyAsync(c => c.IdentificationNumber.ToLower() == identificationNumber.ToLower() && c.Id != excludingId, cancellationToken);
}

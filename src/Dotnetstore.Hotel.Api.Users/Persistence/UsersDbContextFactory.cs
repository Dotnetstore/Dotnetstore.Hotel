using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetstore.Hotel.Api.Users.Persistence;

/// <summary>
/// Used only by EF Core design-time tooling (e.g. `dotnet ef migrations add`), since Aspire injects the
/// real connection string at runtime rather than via configuration files.
/// </summary>
public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=identitydb;Username=postgres;Password=postgres");
        return new UsersDbContext(optionsBuilder.Options);
    }
}

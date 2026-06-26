using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

/// <summary>
/// Used only by EF Core design-time tooling (e.g. `dotnet ef migrations add`), since Aspire injects the
/// real connection string at runtime rather than via configuration files.
/// </summary>
public class HotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=hoteldb;Username=postgres;Password=postgres");
        return new HotelDbContext(optionsBuilder.Options);
    }
}

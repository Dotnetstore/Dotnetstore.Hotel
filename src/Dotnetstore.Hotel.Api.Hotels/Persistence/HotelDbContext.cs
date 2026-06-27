using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;
using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<HotelEntity> Hotels => Set<HotelEntity>();

    public DbSet<Equipment> Equipment => Set<Equipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
}

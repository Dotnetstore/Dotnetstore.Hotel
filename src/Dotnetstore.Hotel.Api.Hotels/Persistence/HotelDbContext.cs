using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;
using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<HotelEntity> Hotels => Set<HotelEntity>();

    public DbSet<Equipment> Equipment => Set<Equipment>();

    public DbSet<Room> Rooms => Set<Room>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
}

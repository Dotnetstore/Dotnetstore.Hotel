using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status).IsRequired();

        builder.HasOne(b => b.Customer)
            .WithMany()
            .HasForeignKey(b => b.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Rooms)
            .WithMany()
            .UsingEntity(j => j.ToTable("booking_rooms"));

        builder.Navigation(b => b.Rooms)
            .HasField("_rooms")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

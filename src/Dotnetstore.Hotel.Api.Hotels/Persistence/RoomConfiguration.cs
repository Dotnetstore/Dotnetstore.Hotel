using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RoomNumber).IsRequired();
        builder.HasIndex(r => r.RoomNumber).IsUnique();

        builder.Property(r => r.BedType).IsRequired();

        builder.Property(r => r.PricePerNight).HasPrecision(10, 2);

        builder.Property(r => r.Status).IsRequired();

        builder.HasMany(r => r.Equipment)
            .WithOne()
            .HasForeignKey(re => re.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(r => r.Equipment)
            .HasField("_equipment")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

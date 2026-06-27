using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public class RoomEquipmentConfiguration : IEntityTypeConfiguration<RoomEquipment>
{
    public void Configure(EntityTypeBuilder<RoomEquipment> builder)
    {
        builder.ToTable("room_equipment");

        builder.HasKey(re => new { re.RoomId, re.EquipmentId });

        builder.HasOne(re => re.Equipment)
            .WithMany()
            .HasForeignKey(re => re.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(re => re.Tags)
            .WithMany()
            .UsingEntity(j => j.ToTable("room_equipment_tags"));

        builder.Navigation(re => re.Tags)
            .HasField("_tags")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HotelEntity = Dotnetstore.Hotel.Api.Hotels.Domain.Hotel;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public class HotelConfiguration : IEntityTypeConfiguration<HotelEntity>
{
    public void Configure(EntityTypeBuilder<HotelEntity> builder)
    {
        builder.ToTable("hotels");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name).IsRequired();
        builder.HasIndex(h => h.Name);

        builder.OwnsOne(h => h.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("street").IsRequired();
            address.Property(a => a.City).HasColumnName("city").IsRequired();
            address.Property(a => a.PostalCode).HasColumnName("postal_code").IsRequired();
            address.Property(a => a.Country).HasColumnName("country").IsRequired();
        });

        builder.OwnsOne(h => h.ContactInfo, contactInfo =>
        {
            contactInfo.Property(c => c.PhoneNumber).HasColumnName("phone_number").IsRequired();
            contactInfo.Property(c => c.Email).HasColumnName("email").IsRequired();
            contactInfo.Property(c => c.Website).HasColumnName("website");
        });

        // EF Core's OwnsMany requires a real entity type, not System.String, so amenities are mapped as a
        // native Postgres array column rather than a child table.
        builder.PrimitiveCollection(h => h.Amenities)
            .HasColumnName("amenities")
            .HasField("_amenities")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

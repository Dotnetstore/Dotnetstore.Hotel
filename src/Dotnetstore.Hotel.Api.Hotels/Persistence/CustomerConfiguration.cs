using Dotnetstore.Hotel.Api.Hotels.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dotnetstore.Hotel.Api.Hotels.Persistence;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CustomerNumber).UseIdentityByDefaultColumn();
        builder.HasIndex(c => c.CustomerNumber).IsUnique();

        builder.Property(c => c.FullName).IsRequired();

        builder.Property(c => c.IdentificationType).IsRequired();

        builder.Property(c => c.IdentificationNumber).IsRequired();
        builder.HasIndex(c => c.IdentificationNumber).IsUnique();

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("street").IsRequired();
            address.Property(a => a.City).HasColumnName("city").IsRequired();
            address.Property(a => a.PostalCode).HasColumnName("postal_code").IsRequired();
            address.Property(a => a.Country).HasColumnName("country").IsRequired();
        });

        builder.Property(c => c.PhoneNumber).IsRequired();

        builder.Property(c => c.Email).IsRequired();

        builder.Property(c => c.Nationality).IsRequired();
    }
}

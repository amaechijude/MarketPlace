using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddress>
{
    public void Configure(EntityTypeBuilder<ShippingAddress> builder)
    {
        builder.ToTable("ShippingAddresses");
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(sa => sa.LastName).IsRequired().HasMaxLength(100);
        builder.Property(sa => sa.Phone).IsRequired().HasMaxLength(16);
        builder.Property(sa => sa.Landmark).IsRequired().HasMaxLength(250);
        builder.Property(sa => sa.Address).IsRequired().HasMaxLength(250);
        builder.Property(sa => sa.City).IsRequired().HasMaxLength(100);
        builder.Property(sa => sa.State).IsRequired().HasMaxLength(50);
    }
}

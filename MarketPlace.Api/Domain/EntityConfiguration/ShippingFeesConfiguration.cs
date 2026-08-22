using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class ShippingFeesConfiguration : IEntityTypeConfiguration<ShippingFee>
{
    public void Configure(EntityTypeBuilder<ShippingFee> builder)
    {
        builder.ToTable("ShippingFees");
        builder.HasKey(sf => sf.Id);
        builder.HasIndex(sf => sf.NormalizedStateName).IsUnique();
        builder.Property(sf => sf.StateName).IsRequired().HasMaxLength(20);
        builder.Property(sf => sf.FeeInKobo).IsRequired();
    }
}

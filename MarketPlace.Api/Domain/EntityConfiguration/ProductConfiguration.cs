using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.ShortDescription).HasMaxLength(250);
        builder.Property(p => p.PriceInKobo).IsRequired();

        // thumbnail
        builder.Property(p => p.ThumbnailUrl).HasMaxLength(500);
        builder.Property(p => p.ThumbnailFileKey).HasMaxLength(300);

        builder.HasQueryFilter(p => p.IsPublished);
    }
}

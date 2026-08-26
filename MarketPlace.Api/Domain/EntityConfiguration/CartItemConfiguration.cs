using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(ci => ci.Id);

        builder.HasQueryFilter(ci => ci.ProductVariant.Product.IsPublished);

        builder.Property(ci => ci.Quantity).IsRequired();

        builder
            .HasOne(ci => ci.ProductVariant)
            .WithMany()
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

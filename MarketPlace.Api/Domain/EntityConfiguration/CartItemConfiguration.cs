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

        builder.HasQueryFilter(ci => ci.Product.IsPublished);

        builder.Property(ci => ci.Quantity).IsRequired();

        builder
            .HasOne(ci => ci.Product)
            .WithMany(pv => pv.CartItems)
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

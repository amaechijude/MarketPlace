using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(c => c.Id);

        builder
            .HasMany(c => c.CartItems)
            .WithOne(i => i.Cart)
            .HasForeignKey(i => i.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User).WithOne(u => u.Cart).HasForeignKey<User>(u => u.Cartid);
    }
}

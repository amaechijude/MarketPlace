using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.ShippingFeeInKobo).IsRequired();

        builder.Property(o => o.TrackingNumber).IsRequired().HasMaxLength(100);

        // relationships
        builder
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // ownship of shipping address — no separate table needed
        builder.OwnsOne(
            o => o.ShippingAddressSnapshot,
            sa =>
            {
                sa.Property(a => a.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ShippingFirstName");

                sa.Property(a => a.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ShippingLastName");

                sa.Property(a => a.Phone)
                    .IsRequired()
                    .HasMaxLength(16)
                    .HasColumnName("ShippingPhone");

                sa.Property(a => a.Address)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ShippingAddress");

                sa.Property(a => a.Landmark)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ShippingLandmark");

                sa.Property(a => a.City)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ShippingCity");

                sa.Property(a => a.State)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ShippingState");

                sa.Property(a => a.Id).IsRequired().HasColumnName("ShippingAddressId");
            }
        );
    }
}

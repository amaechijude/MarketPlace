using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        // Primary Key
        builder.HasKey(v => v.Id);

        // Indexes
        builder.HasIndex(v => v.StoreSlug).IsUnique();
        builder.HasIndex(v => v.UserId).IsUnique();
        builder.HasIndex(v => v.ApprovalStatus);
        builder.HasIndex(v => v.IsActive);
        builder.HasIndex(v => v.CreatedAt);

        // String properties
        builder.Property(v => v.BusinessName).IsRequired().HasMaxLength(256);
        builder.Property(v => v.StoreSlug).IsRequired().HasMaxLength(128);
        builder.Property(v => v.Description).HasMaxLength(1024);
        builder.Property(v => v.LogoUrl).HasMaxLength(2048);
        builder.Property(v => v.SupportEmail).IsRequired().HasMaxLength(256);
        builder.Property(v => v.SupportPhone).HasMaxLength(20);
        builder.Property(v => v.BusinessRegistrationNumber).HasMaxLength(100);
        builder.Property(v => v.BusinessAddress).HasMaxLength(512);
        builder.Property(v => v.Country).HasMaxLength(100);
        builder.Property(v => v.SuspensionReason).HasMaxLength(512);

        // DateTime properties
        builder.Property(v => v.CreatedAt).IsRequired();

        // Enum property
        builder
            .Property(v => v.ApprovalStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(VendorApprovalStatus.Pending);

        // Decimal properties (rating)
        builder.Property(v => v.AverageRating).HasPrecision(3, 2);

        // Foreign Key and Relationships
        builder
            .HasOne(v => v.User)
            .WithOne(u => u.Vendor)
            .HasForeignKey<Vendor>(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One vendor has many products
        builder
            .HasMany(v => v.Products)
            .WithOne(p => p.Vendor)
            .HasForeignKey(p => p.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Table configuration
        builder.ToTable("Vendors");
    }
}

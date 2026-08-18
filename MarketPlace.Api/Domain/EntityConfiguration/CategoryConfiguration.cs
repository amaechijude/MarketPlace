using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);

        builder.Property(c => c.Slug).IsRequired().HasMaxLength(100);

        builder.HasIndex(c => c.Slug).IsUnique();

        // rel
        builder
            .HasMany(c => c.Categories)
            .WithMany(c => c.Categories)
            .UsingEntity(e => e.ToTable("SubCategories"));

        builder.HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId);
    }
}

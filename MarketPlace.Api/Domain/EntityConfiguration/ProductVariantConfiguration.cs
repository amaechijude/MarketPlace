using System.Linq.Expressions;
using System.Text.Json;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketPlace.Api.Domain.EntityConfiguration;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Sku).IsRequired().HasMaxLength(100);
        builder.Property(pv => pv.PriceInKobo).IsRequired();
        builder.Property(pv => pv.StockQuantity).IsRequired();
        builder.HasIndex(pv => pv.Sku).IsUnique();

        builder.HasQueryFilter(pv => pv.Product.IsPublished);

        builder
            .Property(v => v.Attributes)
            .HasColumnType("jsonb")
            .HasConversion(JsonConversions.DictToJson, JsonConversions.JsonToDict)
            .Metadata.SetValueComparer(
                new ValueComparer<Dictionary<string, string>>(
                    (d1, d2) => d1!.SequenceEqual(d2!),
                    d => d.Aggregate(0, (h, kv) => HashCode.Combine(h, kv.Key, kv.Value)),
                    d => new Dictionary<string, string>(d)
                )
            );
    }
}

public static class JsonConversions
{
    public static readonly Expression<Func<Dictionary<string, string>, string>> DictToJson = v =>
        JsonSerializer.Serialize(v, (JsonSerializerOptions?)null);

    public static readonly Expression<Func<string, Dictionary<string, string>>> JsonToDict = v =>
        JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
        ?? new Dictionary<string, string>();
}

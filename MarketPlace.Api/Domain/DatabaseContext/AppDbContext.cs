using JetBrains.Annotations;
using MarketPlace.Api.Domain.DatabaseContext.SeedData;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Domain.DatabaseContext;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

    [UsedImplicitly]
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<ShippingAddress> ShippingAddresses => Set<ShippingAddress>();
    public DbSet<ShippingFee> ShippingFees => Set<ShippingFee>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseAsyncSeeding(SeedAsync).UseSeeding(Seed);
    }

    private static Action<DbContext, bool> Seed =>
        (context, _) =>
        {
            CustomAppRoles.Seed(context);
        };

    private static Func<DbContext, bool, CancellationToken, Task> SeedAsync =>
        async (context, _, ct) =>
        {
            await CustomAppRoles.SeedAsync(context, ct);
        };
}

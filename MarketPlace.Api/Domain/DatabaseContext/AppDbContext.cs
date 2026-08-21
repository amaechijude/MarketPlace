using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Domain.DatabaseContext;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

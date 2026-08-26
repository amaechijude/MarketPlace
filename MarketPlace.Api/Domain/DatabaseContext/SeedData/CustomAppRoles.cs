using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Domain.DatabaseContext.SeedData;

public static class CustomAppRoles
{
    public static void Seed(DbContext context)
    {
        var roleExist = context.Set<Role>().Any();
        if (!roleExist)
        {
            var roles = AllRolesCollection.Select(s => Role.Create(s, Guid.Empty));
            context.Set<Role>().AddRange(roles);
            context.SaveChanges();
        }
        else
        {
            var existingRoleNames = context
                .Set<Role>()
                .Where(r => AllRolesCollection.Contains(r.Name))
                .Select(s => s.Name)
                .ToList();

            var rolesToadd = AllRolesCollection.Where(existingRoleNames.DoesNotContain).ToList();
            if (rolesToadd is not { Count: > 0 })
                return;
            {
                context.Set<Role>().AddRange(rolesToadd.Select(s => Role.Create(s, Guid.Empty)));
                context.SaveChangesAsync();
            }
        }
    }

    public static async Task SeedAsync(DbContext context, CancellationToken ct)
    {
        var roleExist = await context.Set<Role>().AnyAsync(ct);
        if (!roleExist)
        {
            var roles = AllRolesCollection.Select(s => Role.Create(s, Guid.Empty));
            await context.Set<Role>().AddRangeAsync(roles, ct);
            await context.SaveChangesAsync(ct);
        }
        else
        {
            var existingRoleNames = await context
                .Set<Role>()
                .Where(r => AllRolesCollection.Contains(r.Name))
                .Select(s => s.Name)
                .ToListAsync(ct);

            var rolesToadd = AllRolesCollection.Where(existingRoleNames.DoesNotContain).ToList();
            if (rolesToadd is { Count: > 0 })
            {
                await context
                    .Set<Role>()
                    .AddRangeAsync(rolesToadd.Select(s => Role.Create(s, Guid.Empty)), ct);
                await context.SaveChangesAsync(ct);
            }
        }
    }

    // for new roles, just add a const and the append to the list
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Vendor = "Vendor";
    public const string LogisticsManager = "LogisticsManager";
    public const string InventoryManager = "InventoryManager";
    public const string ProductReviewModerator = "ProductReviewModerator";

    private static IReadOnlyCollection<string> AllRolesCollection =>
        [
            SuperAdmin,
            Admin,
            Manager,
            Vendor,
            LogisticsManager,
            InventoryManager,
            ProductReviewModerator,
        ];
}

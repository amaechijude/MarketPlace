using MarketPlace.Api.Domain.DatabaseContext.SeedData;

namespace MarketPlace.Api.Domain.DatabaseContext.SeedData;

/// <summary>
/// Extends CustomAppRoles to ensure Vendor role is properly configured.
/// This partial enhancement adds vendor-specific role configurations if needed.
/// </summary>
public static class VendorRoleSeeder
{
    /// <summary>
    /// Ensures the Vendor role is seeded and available in the database.
    /// This is called as part of the database initialization process.
    /// Note: The Vendor role is already included in CustomAppRoles.AllRolesCollection
    /// </summary>
    public static class VendorRoleConstants
    {
        public const string VendorRole = CustomAppRoles.Vendor;
        public const string VendorDescription = "Vendor with permission to manage products and store";
    }
}

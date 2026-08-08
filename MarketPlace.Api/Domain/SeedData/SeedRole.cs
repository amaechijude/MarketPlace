using MarketPlace.Api.Domain.Entities;

namespace MarketPlace.Api.Domain.SeedData;

public static class SeedRole
{
    private static readonly Guid SuperAdminId = Guid.Parse("019d884e-2c65-721d-a025-4124c7208592");
    private static readonly Guid AdminId = Guid.Parse("019d884f-78f1-7b14-8b0b-79116ef03b82");
    private static readonly Guid ManagerId = Guid.Parse("019d884f-c93c-7500-996a-5af40b07445a");
    private static readonly Guid VendorId = Guid.Parse("019d8850-60a5-7066-85b2-bc78d43f19d8");

    private static readonly Guid ProductReveiwId = Guid.Parse(
        "019dc959-d027-7104-be07-754b1966c487"
    );

    public static readonly IReadOnlyCollection<Role> InitialData =
    [
        new()
        {
            Id = SuperAdminId,
            Name = CustomAppRoles.SuperAdmin,
            CreateAt = DateTimeOffset.Parse("13-Apr-26 7:32:06 PM +00:00"),
        },
        new()
        {
            Id = AdminId,
            Name = CustomAppRoles.Admin,
            CreateAt = DateTimeOffset.Parse("13-Apr-26 7:32:06 PM +00:00"),
        },
        new()
        {
            Id = ManagerId,
            Name = CustomAppRoles.Manager,
            CreateAt = DateTimeOffset.Parse("13-Apr-26 7:32:06 PM +00:00"),
        },
        new()
        {
            Id = VendorId,
            Name = CustomAppRoles.Vendor,
            CreateAt = DateTimeOffset.Parse("13-Apr-26 7:32:06 PM +00:00"),
        },
        new()
        {
            Id = ProductReveiwId,
            Name = CustomAppRoles.ProductReviewModerator,
            CreateAt = DateTimeOffset.Parse("26-Apr-26 3:26:37 PM +00:00"),
        },
    ];
}

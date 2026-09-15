using System.ComponentModel.DataAnnotations;

namespace MarketPlace.Api.Domain.Entities;

/// <summary>
/// Represents a vendor in the multi-vendor marketplace platform.
/// Each vendor is associated with a user account and manages their own product catalog and store.
/// </summary>
public sealed class Vendor
{
    /// <summary>
    /// Unique identifier for the vendor (GUID v7).
    /// </summary>
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    /// <summary>
    /// Foreign key to the User entity (one-to-one relationship).
    /// </summary>
    public Guid UserId { get; private init; }

    /// <summary>
    /// User entity navigation property.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Official business name of the vendor.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string BusinessName { get; private set; } = string.Empty;

    /// <summary>
    /// Unique, URL-friendly slug for the vendor's store.
    /// Used in store URLs and must be globally unique.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string StoreSlug { get; private set; } = string.Empty;

    /// <summary>
    /// Detailed description of the vendor's business and offerings.
    /// </summary>
    [MaxLength(1024)]
    public string? Description { get; private set; }

    /// <summary>
    /// URL to the vendor's store logo/branding image.
    /// </summary>
    [MaxLength(2048)]
    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Primary email address for vendor support inquiries.
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string SupportEmail { get; private set; } = string.Empty;

    /// <summary>
    /// Phone number for vendor customer support.
    /// </summary>
    [MaxLength(20)]
    public string? SupportPhone { get; private set; }

    /// <summary>
    /// Current approval status of the vendor (Pending, Approved, or Suspended).
    /// </summary>
    public VendorApprovalStatus ApprovalStatus { get; private set; } = VendorApprovalStatus.Pending;

    /// <summary>
    /// Indicates whether the vendor account is actively operational.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Business registration number or tax ID (optional, for compliance).
    /// </summary>
    [MaxLength(100)]
    public string? BusinessRegistrationNumber { get; private set; }

    /// <summary>
    /// Physical address of the business headquarters.
    /// </summary>
    [MaxLength(512)]
    public string? BusinessAddress { get; private set; }

    /// <summary>
    /// Country where the vendor operates (for compliance and shipping).
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; private set; }

    /// <summary>
    /// Timestamp when the vendor account was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private init; }

    /// <summary>
    /// Timestamp of the last account update.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// User ID who last updated the vendor record (audit trail).
    /// </summary>
    public Guid? UpdatedBy { get; private set; }

    /// <summary>
    /// Reason for vendor suspension (if applicable).
    /// </summary>
    [MaxLength(512)]
    public string? SuspensionReason { get; private set; }

    /// <summary>
    /// Date when suspension was applied (if applicable).
    /// </summary>
    public DateTimeOffset? SuspendedAt { get; private set; }

    /// <summary>
    /// Rating/score based on customer reviews and order fulfillment.
    /// </summary>
    public decimal? AverageRating { get; private set; }

    /// <summary>
    /// Number of total customer reviews for this vendor.
    /// </summary>
    public int TotalReviews { get; private set; }

    /// <summary>
    /// Navigation property for vendor's products.
    /// </summary>
    public ICollection<Product> Products { get; set; } = [];

    /// <summary>
    /// Creates a new vendor instance for onboarding.
    /// </summary>
    public static Vendor Create(
        Guid userId,
        string businessName,
        string storeSlug,
        string supportEmail,
        DateTimeOffset createdAt
    )
    {
        return new Vendor
        {
            UserId = userId,
            BusinessName = businessName,
            StoreSlug = storeSlug.ToLowerInvariant(),
            SupportEmail = supportEmail,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
            ApprovalStatus = VendorApprovalStatus.Pending,
        };
    }

    /// <summary>
    /// Updates vendor profile information.
    /// </summary>
    public void UpdateProfile(
        string? description,
        string? supportPhone,
        string? logoUrl,
        string? businessAddress,
        string? country,
        Guid? updatedBy,
        DateTimeOffset updatedAt
    )
    {
        Description = description ?? Description;
        SupportPhone = supportPhone ?? SupportPhone;
        LogoUrl = logoUrl ?? LogoUrl;
        BusinessAddress = businessAddress ?? BusinessAddress;
        Country = country ?? Country;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Approves the vendor for marketplace operations.
    /// </summary>
    public void Approve(Guid approvedBy, DateTimeOffset approvalDate)
    {
        ApprovalStatus = VendorApprovalStatus.Approved;
        UpdatedBy = approvedBy;
        UpdatedAt = approvalDate;
    }

    /// <summary>
    /// Suspends the vendor from the marketplace with a reason.
    /// </summary>
    public void Suspend(
        string reason,
        Guid suspendedBy,
        DateTimeOffset suspensionDate
    )
    {
        ApprovalStatus = VendorApprovalStatus.Suspended;
        IsActive = false;
        SuspensionReason = reason;
        SuspendedAt = suspensionDate;
        UpdatedBy = suspendedBy;
        UpdatedAt = suspensionDate;
    }

    /// <summary>
    /// Reactivates a suspended vendor.
    /// </summary>
    public void Reactivate(Guid reactivatedBy, DateTimeOffset reactivationDate)
    {
        ApprovalStatus = VendorApprovalStatus.Approved;
        IsActive = true;
        SuspensionReason = null;
        SuspendedAt = null;
        UpdatedBy = reactivatedBy;
        UpdatedAt = reactivationDate;
    }

    /// <summary>
    /// Updates vendor ratings based on customer reviews.
    /// </summary>
    public void UpdateRating(decimal newAverageRating, int totalReviews)
    {
        AverageRating = newAverageRating;
        TotalReviews = totalReviews;
    }
}

/// <summary>
/// Enumeration of possible vendor approval statuses.
/// </summary>
public enum VendorApprovalStatus
{
    /// <summary>Vendor registration is pending review.</summary>
    Pending = 0,

    /// <summary>Vendor has been approved and can operate.</summary>
    Approved = 1,

    /// <summary>Vendor account has been suspended.</summary>
    Suspended = 2,
}

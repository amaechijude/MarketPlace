using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Domain.Entities;

public sealed class Vendor
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public Guid UserId { get; private init; }

    public User? User { get; set; }
    public string BusinessName { get; private set; } = string.Empty;
    public string StoreSlug { get; private set; } = string.Empty;

    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public string SupportEmail { get; private set; } = string.Empty;

    public string? SupportPhone { get; private set; }

    public VendorApprovalStatus ApprovalStatus { get; private set; } = VendorApprovalStatus.Pending;

    public bool IsActive { get; private set; } = true;

    public string? BusinessRegistrationNumber { get; private set; }
    public string? BusinessAddress { get; private set; }
    public string? Country { get; private set; }

    public DateTimeOffset CreatedAt { get; private init; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid? UpdatedBy { get; private set; }
    public string? SuspensionReason { get; private set; }

    public DateTimeOffset? SuspendedAt { get; private set; }

    public decimal? AverageRating { get; private set; }

    public int TotalReviews { get; private set; }

    public ICollection<Product> Products { get; set; } = [];
    public string Email { get; internal set; } = string.Empty;

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

    public void Approve(Guid approvedBy, DateTimeOffset approvalDate)
    {
        ApprovalStatus = VendorApprovalStatus.Approved;
        UpdatedBy = approvedBy;
        UpdatedAt = approvalDate;
    }

    public void Suspend(string reason, Guid suspendedBy, DateTimeOffset suspensionDate)
    {
        ApprovalStatus = VendorApprovalStatus.Suspended;
        IsActive = false;
        SuspensionReason = reason;
        SuspendedAt = suspensionDate;
        UpdatedBy = suspendedBy;
        UpdatedAt = suspensionDate;
    }

    public void Reactivate(Guid reactivatedBy, DateTimeOffset reactivationDate)
    {
        ApprovalStatus = VendorApprovalStatus.Approved;
        IsActive = true;
        SuspensionReason = null;
        SuspendedAt = null;
        UpdatedBy = reactivatedBy;
        UpdatedAt = reactivationDate;
    }

    public void UpdateRating(decimal newAverageRating, int totalReviews)
    {
        AverageRating = newAverageRating;
        TotalReviews = totalReviews;
    }

    public void UpdateBusinessRegistrationNumber(
        string? businessRegistrationNumber,
        Guid updatedBy,
        DateTimeOffset updatedAt
    )
    {
        BusinessRegistrationNumber = businessRegistrationNumber;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }
}

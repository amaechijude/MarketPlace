# Vendor Entity — EF Core Design Spec

## Context

Domain model for the vendor/seller side of a multivendor marketplace (Jumia-style: independent
third-party sellers list into one shared storefront, platform takes a commission, sellers carry
inventory risk and must pass KYC before selling). This spec covers the `Vendor` aggregate and its
directly related entities only — not `Product`, `Order`, or the identity/auth model.

Target: EF Core 8, code-first, Fluent API configuration (no data annotations beyond `[Timestamp]`
and `[Owned]`).

## Files to create

### `Domain/Vendors/Vendor.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiVendorPlatform.Domain.Vendors;

public enum VendorStatus
{
    PendingApproval = 0,
    Active = 1,
    Suspended = 2,
    Rejected = 3,
    Closed = 4
}

public enum VendorBusinessType
{
    Individual = 0,
    SoleProprietorship = 1,
    LimitedLiabilityCompany = 2,
    Corporation = 3,
    Partnership = 4
}

// Document types vary by country and business type (e.g. CAC certificate + TIN
// for a registered Nigerian company vs. just a national ID for an individual
// seller elsewhere). Kept as an open enum/string rather than fixed columns on
// Vendor so new markets can require new document types without a migration
// touching the core entity.
public enum VendorDocumentType
{
    NationalId = 0,
    BusinessRegistrationCertificate = 1,   // e.g. Nigeria's CAC Form 2&7 / Status Report
    TaxIdentificationCertificate = 2,
    BankReferenceLetter = 3,
    ProofOfAddress = 4,
    Other = 99
}

public enum VendorDocumentStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class Vendor
{
    public Guid Id { get; set; }

    // Link to the auth/identity user who owns this store.
    // Kept separate so login credentials aren't mixed with business data.
    public string OwnerId { get; set; } = default!;
    public ApplicationUser Owner { get; set; } = default!;

    // Storefront identity
    public string StoreName { get; set; } = default!;
    public string Slug { get; set; } = default!;              // unique, URL-safe, e.g. "acme-electronics"
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }

    // Contact info (can differ from the owner's login email/phone)
    public string ContactEmail { get; set; } = default!;
    public string ContactPhone { get; set; } = default!;

    // Drives which KYC document set and payout format apply (ISO 3166-1 alpha-2,
    // e.g. "NG"). Kept separate from Address.Country, which is free-text/display.
    public string CountryCode { get; set; } = default!;

    // Legal / compliance — these are the *parsed values* extracted from the
    // reviewed documents below, kept here for fast lookups/search. The
    // documents themselves are the source of truth for verification.
    public string? BusinessRegistrationNumber { get; set; }
    public string? TaxIdentificationNumber { get; set; }
    public VendorBusinessType BusinessType { get; set; }

    // Lifecycle / approval workflow
    public VendorStatus Status { get; set; } = VendorStatus.PendingApproval;
    public bool IsVerified { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ApprovedByUserId { get; set; }

    // Mirrors the step-by-step onboarding wizard (Shop / Business / Shipping /
    // Payment / Additional info), each independently completed or pending.
    public VendorOnboardingChecklist Onboarding { get; set; } = new();

    // Commerce terms
    public decimal? CommissionRateOverride { get; set; }      // null => fall back to platform default
    public string DefaultCurrency { get; set; } = "USD";

    // Denormalized, eventually-consistent aggregates.
    // Updated by a background job/event handler, not computed on every read.
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int TotalOrders { get; set; }

    // Owned value object — always loaded with the vendor, no separate table needed
    public VendorAddress Address { get; set; } = default!;

    // 1:1 sensitive data, kept in its own table for isolation/security
    public VendorPayoutAccount? PayoutAccount { get; set; }

    // KYC documents — each uploaded file has its own review/approval state
    public ICollection<VendorDocument> Documents { get; set; } = new List<VendorDocument>();

    // Navigation
    public ICollection<Product> Products { get; set; } = new List<Product>();

    // Audit
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;
}

[Owned]
public class VendorAddress
{
    public string Line1 { get; set; } = default!;
    public string? Line2 { get; set; }
    public string City { get; set; } = default!;
    public string? State { get; set; }
    public string Country { get; set; } = default!;
    public string? PostalCode { get; set; }
}

public class VendorPayoutAccount
{
    public Guid Id { get; set; }

    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = default!;

    public string BankName { get; set; } = default!;
    public string AccountNumberEncrypted { get; set; } = default!;  // store encrypted, never plaintext
    public string AccountHolderName { get; set; } = default!;       // must match the bank reference letter
    public string? RoutingOrSwiftCode { get; set; }                 // e.g. NUBAN in Nigeria, IFSC in India, etc.
}

// One row per uploaded KYC file (National ID, CAC certificate, TIN certificate,
// bank reference letter, ...). A Vendor can have several, and each is reviewed
// independently — a rejected National ID shouldn't block an already-approved TIN.
public class VendorDocument
{
    public Guid Id { get; set; }

    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = default!;

    public VendorDocumentType DocumentType { get; set; }
    public string FileUrl { get; set; } = default!;

    public VendorDocumentStatus Status { get; set; } = VendorDocumentStatus.Pending;
    public string? RejectionReason { get; set; }

    public DateTime UploadedAtUtc { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string? ReviewedByUserId { get; set; }
}

// Owned type mirroring the onboarding wizard's five sections. Persisted
// (rather than computed on the fly) because admin dashboards need to filter
// "vendors stuck on Payment Information" etc. without recomputing state from
// scratch on every query.
[Owned]
public class VendorOnboardingChecklist
{
    public bool ShopInfoCompleted { get; set; }
    public bool BusinessInfoCompleted { get; set; }
    public bool ShippingInfoCompleted { get; set; }
    public bool PaymentInfoCompleted { get; set; }
    public bool AdditionalInfoCompleted { get; set; }

    public bool IsComplete =>
        ShopInfoCompleted && BusinessInfoCompleted && ShippingInfoCompleted
        && PaymentInfoCompleted && AdditionalInfoCompleted;
}
```

### `Domain/Vendors/VendorConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MultiVendorPlatform.Domain.Vendors;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.StoreName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(v => v.Slug)
            .HasMaxLength(180)
            .IsRequired();
        builder.HasIndex(v => v.Slug).IsUnique();

        builder.Property(v => v.ContactEmail)
            .HasMaxLength(256)
            .IsRequired();
        builder.HasIndex(v => v.ContactEmail);

        // Store enum as string for readability in the DB / easier debugging
        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.HasIndex(v => v.Status);  // admin dashboards filter heavily by status

        builder.Property(v => v.CommissionRateOverride)
            .HasColumnType("decimal(5,2)");

        builder.Property(v => v.AverageRating)
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0m);

        builder.Property(v => v.DefaultCurrency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(v => v.CountryCode)
            .HasMaxLength(2)
            .IsRequired();
        builder.HasIndex(v => v.CountryCode);  // KYC rules and payout formats branch on this

        // Owned type — mapped into the same Vendors table by default
        builder.OwnsOne(v => v.Address, a =>
        {
            a.Property(x => x.Line1).HasMaxLength(200).IsRequired();
            a.Property(x => x.City).HasMaxLength(100).IsRequired();
            a.Property(x => x.Country).HasMaxLength(100).IsRequired();
            a.Property(x => x.PostalCode).HasMaxLength(20);
        });

        // Onboarding checklist — also mapped into the Vendors table
        builder.OwnsOne(v => v.Onboarding);

        // Sensitive payout data lives in its own table
        builder.HasOne(v => v.PayoutAccount)
            .WithOne(p => p.Vendor)
            .HasForeignKey<VendorPayoutAccount>(p => p.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Owner)
            .WithMany()
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.Products)
            .WithOne(p => p.Vendor)
            .HasForeignKey(p => p.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(v => v.Documents)
            .WithOne(d => d.Vendor)
            .HasForeignKey(d => d.VendorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft-deleted vendors are hidden from normal queries automatically
        builder.HasQueryFilter(v => !v.IsDeleted);

        builder.Property(v => v.RowVersion).IsRowVersion();
    }
}

public class VendorDocumentConfiguration : IEntityTypeConfiguration<VendorDocument>
{
    public void Configure(EntityTypeBuilder<VendorDocument> builder)
    {
        builder.ToTable("VendorDocuments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(d => d.FileUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.RejectionReason)
            .HasMaxLength(500);

        // Reviewers filter the queue by vendor + pending status
        builder.HasIndex(d => new { d.VendorId, d.Status });
    }
}
```

## Design decisions & rationale

- **Owner vs. business profile split**: `Vendor.OwnerId` points at the identity/auth user; all
  storefront and legal data lives on `Vendor` itself, so login credentials never mix with
  business data.
- **Payout data isolated**: `VendorPayoutAccount` is a separate 1:1 table, not columns on
  `Vendor`, so bank details can have different access control/auditing than public storefront
  fields. `AccountNumberEncrypted` must be encrypted at the application layer before it's saved —
  never persist plaintext.
- **KYC documents are files with independent review state**: `VendorDocument` is one row per
  upload (National ID, business registration certificate, TIN certificate, bank reference
  letter), each with its own `Pending/Approved/Rejected` status and rejection reason, because a
  rejected document shouldn't block an already-approved one.
- **Document requirements are data-driven, not schema-driven**: `VendorDocumentType` is a shared
  enum rather than fixed columns, since required documents vary by `CountryCode` and
  `BusinessType` (e.g. Nigeria requires a CAC certificate; other markets won't). Per-country
  requirement rules belong in application/business logic, not the schema.
- **Onboarding checklist mirrors the actual wizard UI**: `VendorOnboardingChecklist` (Shop /
  Business / Shipping / Payment / Additional info, each a bool) is persisted rather than computed
  on the fly, so admin dashboards can filter "vendors stuck on Payment Information" without
  recomputing state on every query.
- **Status enum, not a bool**: `VendorStatus` covers the real approval lifecycle
  (PendingApproval → Active/Rejected, plus Suspended/Closed later), stored as a string for
  DB readability.
- **Soft delete**: `IsDeleted` + global query filter, since vendor history must stay attached to
  past orders — never hard-delete a vendor.
- **Optimistic concurrency**: `RowVersion` guards against races between admin approval actions
  and vendor self-edits happening at the same time.
- **Denormalized aggregates**: `AverageRating`, `RatingCount`, `TotalOrders` are
  eventually-consistent, updated by a background job/event handler — not computed live on every
  storefront page load.
- **`CommissionRateOverride` is nullable**: most vendors inherit a platform-wide default;
  this only holds per-vendor negotiated rates.

## Open items for the implementing agent

1. Register both entities and their `IEntityTypeConfiguration` classes in the `DbContext`
   (`modelBuilder.ApplyConfigurationsFromAssembly(...)` or explicit `ApplyConfiguration` calls),
   then generate the initial migration.
2. Implement the field-level encryption for `VendorPayoutAccount.AccountNumberEncrypted`
   (e.g. `ProtectedData`, Azure Key Vault, or a custom EF value converter) — this is referenced
   but not implemented in the spec above.
3. Define the per-`CountryCode`/`BusinessType` required-document rule set (likely a small
   lookup table or config, e.g. `NG` + `Company` → `[NationalId, BusinessRegistrationCertificate,
  TaxIdentificationCertificate, BankReferenceLetter]`) and the validation that blocks a vendor
   from reaching `Active` status until all required documents are `Approved`.
4. Build the KYC review workflow/service: endpoints or admin actions to list pending
   `VendorDocument`s, approve/reject with a reason, and flip `Vendor.IsVerified`/`Status`
   accordingly.
5. Wire up the background job that recomputes `AverageRating`, `RatingCount`, and `TotalOrders`
   from the orders/reviews data (event-driven or scheduled — implementer's choice).
6. `ApplicationUser` and `Product` are referenced but out of scope here — confirm they already
   exist in the codebase with matching types before compiling.
7. Add unit/integration tests for: slug uniqueness, soft-delete query filter behavior, and the
   cascade delete from `Vendor` to `VendorDocument`.

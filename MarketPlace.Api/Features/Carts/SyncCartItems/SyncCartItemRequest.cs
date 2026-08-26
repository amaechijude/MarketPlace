using System.ComponentModel.DataAnnotations;
using MarketPlace.Api.Common.ValidationAttributes;

namespace MarketPlace.Api.Features.Carts.SyncCartItems;

public sealed record SyncCartItemRequest(List<SyncBody> CLientItems);

public sealed record SyncBody([IsValidGuid] Guid ProductVariantId, [Range(1, 100)] int Quantity);

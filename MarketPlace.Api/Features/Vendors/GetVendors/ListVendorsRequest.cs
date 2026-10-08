namespace MarketPlace.Api.Features.Vendors.DTOs;

public sealed record ListVendorsRequest(Guid? Cursor = null, int PageSize = 30);

using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.ShippingAddresses.CreateShippingAddress;

public sealed class CreateAddressHandler(AppDbContext context, TimeProvider timeProvider)
    : IRequestHandler
{
    private const int MaxAddressCount = 5;

    public async Task<ApiResponse<AddressResponse>> HandleAsync(
        Guid userId,
        CreateAddressRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await context
            .Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(s => new
            {
                s.Id,
                ShippingAddresses = s.ShippingAddresses.Select(sa => new { sa.IsDefault }).ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return ApiResponse<AddressResponse>.NotFound("User not found");

        if (user.ShippingAddresses is { Count: >= MaxAddressCount })
        {
            return ApiResponse<AddressResponse>.BadRequest(
                "Maximum address limit reached. Delete some addresses first."
            );
        }

        var newAddress = Create(userId, request, timeProvider.GetUtcNow());
        // check if default exist
        var defaultAddress = user.ShippingAddresses.FirstOrDefault(s => s.IsDefault);
        if (defaultAddress is null)
            newAddress.MarkAsDefault();

        context.ShippingAddresses.Add(newAddress);

        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<AddressResponse>.Success(Create(newAddress));
    }

    private static ShippingAddress Create(
        Guid userId,
        CreateAddressRequest request,
        DateTimeOffset createdAt
    ) =>
        new(userId)
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Address = request.Address,
            Landmark = request.Landmark,
            City = request.City,
            State = request.State,
            CreatedAt = createdAt,
        };

    private static AddressResponse Create(ShippingAddress newAddress) =>
        new(
            newAddress.Id,
            newAddress.FirstName,
            newAddress.LastName,
            newAddress.Phone,
            newAddress.Address,
            newAddress.Landmark,
            newAddress.City,
            newAddress.State
        );
}

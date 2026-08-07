using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;

namespace MarketPlace.Api.Features.Users.Register;

public sealed class RegisterUserHandler(AppDbContext context) : IRequestHandler
{
    public async Task<object> CreateAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(100, cancellationToken);
        return request;
    }
}

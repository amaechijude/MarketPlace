using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.Register;

namespace MarketPlace.Api.Features.Users;

public sealed class UserEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("users").WithTags("Users");

        RegisterEndpoint.Map(group);
    }
}

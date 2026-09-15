using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.ForgotPassword;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Features.Users.Logout;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Api.Infrastucture.Auth;

namespace MarketPlace.Api.Features.Users;

public sealed class UserEndpoints : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("auth").WithTags("Auth");

        RegisterEndpoint.Map(group);
        LoginEnpoint.Map(group);
        ForgotPasswordEndpoint.Map(group);
        LogoutEndpoint.Map(group);
    }
}

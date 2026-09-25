using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Auth.ForgotPassword;
using MarketPlace.Api.Features.Auth.Login;
using MarketPlace.Api.Features.Auth.Logout;
using MarketPlace.Api.Features.Auth.Register;

namespace MarketPlace.Api.Features.Auth;

public sealed class AuthEndpoints : IRequestEndpoints
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

using System.Security.Claims;
using System.Text.Encodings.Web;
using MarketPlace.Api.Common.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class AuthSessionHandler(
    IOptionsMonitor<AuthSessionOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuthSessionStore authSessionstore
) : AuthenticationHandler<AuthSessionOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.ExtractToken();

        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.NoResult();

        var session = await authSessionstore.GetSessionAsync(token, Context.RequestAborted);
        if (session is null)
            return AuthenticateResult.NoResult();

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            .. session.Roles.Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        ClaimsIdentity identity = new(claims, Scheme.Name);
        ClaimsPrincipal principal = new(identity);

        AuthenticationTicket ticket = new(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await Results.Problem(statusCode: StatusCodes.Status401Unauthorized).ExecuteAsync(Context);
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        await Results.Problem(statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(Context);
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Infrastucture.Auth;

public sealed class AuthSessionHandler(
    IOptionsMonitor<AuthSessionOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AuthSessionstore authSessionstore
) : AuthenticationHandler<AuthSessionOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = AuthSessionOptions.ExtractToken(Request);
        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.NoResult();

        var session = await authSessionstore.GetSessionAsync(token, Context.RequestAborted);
        if (session is null)
            return AuthenticateResult.Fail("Invalid or expired session");

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            .. session.Roles.Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        await Context
            .RequestServices.GetRequiredService<IProblemDetailsService>()
            .TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = Context,
                    ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                    },
                }
            );
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        await Context
            .RequestServices.GetRequiredService<IProblemDetailsService>()
            .TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = Context,
                    ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
                    {
                        Status = StatusCodes.Status403Forbidden,
                    },
                }
            );
    }
}

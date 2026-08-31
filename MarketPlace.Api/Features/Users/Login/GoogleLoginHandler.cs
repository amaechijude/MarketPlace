using Google.Apis.Auth;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Infrastucture.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MarketPlace.Api.Features.Users.Login;

public sealed class GoogleLoginHandler(
    IAuthSessionStore authSessionstore,
    AppDbContext context,
    IOptions<GoogleSettings> googleOptions,
    ILogger<GoogleLoginHandler> logger,
    TimeProvider timeProvider
) : IScopedRequestHandler
{
    public async Task<LoginResponse> HandleAsync(
        GoogleLoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var payload = await ValidateGoogleJwtAsync(request.Jwt, googleOptions.Value.ClientId);
        if (payload is null)
            return LoginResponse.Fail("Invalid or expired session");

        // Google accounts whose email isn't verified should not be trusted
        if (!payload.EmailVerified)
            return LoginResponse.Fail("Google account email is not verified");

        var user = await ResolveUserAsync(payload, cancellationToken);

        var (token, expiresOn) = await authSessionstore.CreateAsync(
            user.Id,
            user.Roles.Select(r => r.Name),
            cancellationToken
        );

        return LoginResponse.Success(token, expiresOn);
    }

    private async Task<User> ResolveUserAsync(
        GoogleJsonWebSignature.Payload payload,
        CancellationToken cancellationToken
    )
    {
        var normalisedEmail = EmailNormalizer.Normalize(payload.Email);

        var user = await context
            .Users.AsNoTracking()
            .Include(u => u.Roles)
            .Where(u => u.NormalizedEmail == normalisedEmail)
            .FirstOrDefaultAsync(cancellationToken);

        switch (user)
        {
            case null:
                // Brand new user
                user = User.Create(payload, timeProvider.GetUtcNow());
                context.Users.Add(user);
                await context.SaveChangesAsync(cancellationToken);

                return user;
            case { EmailConfrimed: true }:
                return user;
        }

        user.MarkEmailConfirmed();
        await context.SaveChangesAsync(cancellationToken);

        return user;
    }

    private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleJwtAsync(
        string jwt,
        string clientId
    )
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId],
                IssuedAtClockTolerance = TimeSpan.FromSeconds(60),
            };
            return await GoogleJsonWebSignature.ValidateAsync(jwt, settings);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Google JWT validation failed: {Message}", ex.Message);
            return null;
        }
    }
}

using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Infrastucture.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Users.Login;

public sealed class EmailLoginHandler(
    AuthSessionstore authSessionstore,
    AppDbContext context,
    IPasswordHasher<User> hasher
) : IRequestHandler
{
    public async Task<LoginResponse> HandleAsync(
        EmailLoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = EmailNormalizer.Normalize(request.Email);

        var user = await context
            .Users.Include(u => u.Roles)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email, cancellationToken);

        if (user is null)
            return LoginResponse.Fail("Invalid Credentials");

        var validate = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (validate == PasswordVerificationResult.Failed)
            return LoginResponse.Fail("Invalid Credentials");

        var ttl = AuthSessionOptions.DefaultTtl;

        var token = await authSessionstore.CreateAsync(
            new AuthSessionRecord(user.Id, user.Roles.Select(r => r.Name)),
            ttl,
            cancellationToken
        );

        return LoginResponse.Success(token, ttl);
    }
}

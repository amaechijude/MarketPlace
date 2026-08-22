using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Infrastucture.Auth;
using MarketPlace.Api.Infrastucture.OtpValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Users.Login;

public sealed class EmailLoginHandler(
    AuthSessionstore authSessionstore,
    AppDbContext context,
    IPasswordHasher<User> hasher,
    VerificationCodeManager verificationCodeManager
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

        if (user is { EmailConfrimed: true })
        {
            var (token, ttl) = await authSessionstore.CreateAsync(
                user.Id,
                user.Roles.Select(r => r.Name),
                cancellationToken
            );

            return LoginResponse.Success(token, ttl);
        }
        var otpId = await verificationCodeManager.GenerateAndDispatchOtp(
            user.Id,
            email,
            OtpType.Login,
            cancellationToken
        );

        return LoginResponse.Success(otpId.ToString(), TimeSpan.Zero);
    }

    public async Task<LoginResponse> VerifyOtpAsync(
        EmailLoginVerifyOtpRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await verificationCodeManager.ValidateOtp(
            request.OtpInput,
            request.OtpId,
            OtpType.Login,
            cancellationToken
        );

        if (!result.IsValid || result.UserId.IsEmpty)
            return LoginResponse.Fail();

        var user = await context.Users.FindAsync(
            [result.UserId],
            cancellationToken: cancellationToken
        );

        switch (user)
        {
            case null:
                return LoginResponse.Fail();
            case { EmailConfrimed: false }:
                user.MarkEmailConfirmed();
                await context.SaveChangesAsync(cancellationToken);
                break;
        }

        // login
        var (token, ttl) = await authSessionstore.CreateAsync(user.Id, [], cancellationToken);

        return LoginResponse.Success(token, ttl);
    }
}

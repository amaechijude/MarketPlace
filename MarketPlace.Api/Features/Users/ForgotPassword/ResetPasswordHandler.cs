using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Infrastucture.Auth;
using MarketPlace.Api.Infrastucture.OtpValidation;
using Microsoft.AspNetCore.Identity;

namespace MarketPlace.Api.Features.Users.ForgotPassword;

public sealed class ResetPasswordHandler(
    AppDbContext context,
    VerificationCodeManager verificationCodeManager,
    IAuthSessionStore authSessionstore,
    IPasswordHasher<User> hasher
) : IScopedRequestHandler
{
    public async Task<LoginResponse> HandleAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await verificationCodeManager.ValidateOtp(
            request.OtpInput,
            request.OtpId,
            OtpType.ResetPassword,
            cancellationToken
        );

        if (!result.IsValid || result.UserId.IsEmpty)
            return LoginResponse.Fail();

        var user = await context.Users.FindAsync(
            [result.UserId],
            cancellationToken: cancellationToken
        );

        if (user is null)
            return LoginResponse.Fail();

        var hash = hasher.HashPassword(user, request.Password);
        user.AddPasswordHash(hash);
        await context.SaveChangesAsync(cancellationToken);

        // login
        var (token, expiresOn) = await authSessionstore.CreateAsync(
            user.Id,
            user.Roles.Select(r => r.Name),
            cancellationToken
        );

        return LoginResponse.Success(token, expiresOn);
    }
}

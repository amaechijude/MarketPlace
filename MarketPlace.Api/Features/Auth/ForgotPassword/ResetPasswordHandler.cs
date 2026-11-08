using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.Auth.Login;
using MarketPlace.Api.Infrastucture.AuthInfrastructure;
using MarketPlace.Api.Infrastucture.OtpService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Auth.ForgotPassword;

public sealed class ResetPasswordHandler(
    AppDbContext context,
    OtpEmailHandler otpEmailService,
    IAuthSessionStore authSessionstore,
    IPasswordHasher<User> hasher
) : IScopedRequestHandler
{
    public async Task<LoginResponse> HandleAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = EmailNormalizer.Normalize(request.Email);
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.NormalizedEmail == email,
            cancellationToken: cancellationToken
        );

        if (user is null)
            return LoginResponse.Fail();

        var isValid = await otpEmailService.ValidateOtpAsync(
            request.OtpInput,
            OtpType.ResetPassword,
            user.Id,
            cancellationToken
        );

        if (!isValid)
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

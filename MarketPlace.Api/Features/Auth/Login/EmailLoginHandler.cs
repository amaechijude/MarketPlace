using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.Auth.Register;
using MarketPlace.Api.Infrastucture.AuthInfrastructure;
using MarketPlace.Api.Infrastucture.OtpService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Auth.Login;

public sealed class EmailLoginHandler(
    IAuthSessionStore authSessionstore,
    AppDbContext context,
    IPasswordHasher<User> hasher,
    OtpEmailHandler otpEmailService
) : IScopedRequestHandler
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
            var (token, expiresOn) = await authSessionstore.CreateAsync(
                user.Id,
                user.Roles.Select(r => r.Name),
                cancellationToken
            );

            return LoginResponse.Success(token, expiresOn);
        }
        await otpEmailService.GenerateAndEnqueueAsync(
            user.Id,
            user.Email,
            OtpType.Login,
            cancellationToken
        );

        return LoginResponse.Success();
    }

    public async Task<LoginResponse> VerifyOtpAsync(
        VerifyOtpApiRequest request,
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
            OtpType.Login,
            user.Id,
            cancellationToken
        );

        if (!isValid)
            return LoginResponse.Fail();

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
        var (token, expiresOn) = await authSessionstore.CreateAsync(user.Id, [], cancellationToken);

        return LoginResponse.Success(token, expiresOn);
    }
}

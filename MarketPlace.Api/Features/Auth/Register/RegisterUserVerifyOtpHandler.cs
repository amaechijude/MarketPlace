using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Features.Auth.Login;
using MarketPlace.Api.Infrastucture.AuthInfrastructure;
using MarketPlace.Api.Infrastucture.OtpService;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Auth.Register;

public sealed class RegisterUserVerifyOtpHandler(
    AppDbContext context,
    OtpEmailHandler otpEmailService,
    IAuthSessionStore authSessionstore
) : IScopedRequestHandler
{
    /// <summary>
    /// Verify otp and login
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<LoginResponse> HandleAsync(
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

        if (user.EmailConfrimed)
        {
            // login
            var (tok, expires) = await authSessionstore.CreateAsync(user.Id, [], cancellationToken);

            return LoginResponse.Success(tok, expires);
        }

        var isValid = await otpEmailService.ValidateOtpAsync(
            request.OtpInput,
            OtpType.Register,
            user.Id,
            cancellationToken
        );

        if (!isValid)
            return LoginResponse.Fail();

        if (user is { EmailConfrimed: false })
        {
            user.MarkEmailConfirmed();
            await context.SaveChangesAsync(cancellationToken);
        }

        // login
        var (token, expiresOn) = await authSessionstore.CreateAsync(user.Id, [], cancellationToken);

        return LoginResponse.Success(token, expiresOn);
    }
}

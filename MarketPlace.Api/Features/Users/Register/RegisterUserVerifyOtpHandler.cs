using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Infrastucture.Auth;
using MarketPlace.Api.Infrastucture.OtpValidation;

namespace MarketPlace.Api.Features.Users.Register;

public sealed class RegisterUserVerifyOtpHandler(
    AppDbContext context,
    VerificationCodeManager verificationCodeManager,
    AuthSessionstore authSessionstore
) : IRequestHandler
{
    /// <summary>
    /// Verify otp and login
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<LoginResponse> HandleAsync(
        RegisterUserVerifyOtpRequest request,
        CancellationToken cancellationToken
    )
    {
        OtpValidationResult result = await verificationCodeManager.ValidateOtp(
            request.OtpInput,
            request.OtpId,
            OtpType.Register,
            cancellationToken
        );
        if (!result.IsValid || result.UserId.IsEmpty())
            return LoginResponse.Fail();

        User? user = await context.Users.FindAsync(
            [result.UserId],
            cancellationToken: cancellationToken
        );

        if (user is null)
            return LoginResponse.Fail();

        if (!user.EmailConfrimed)
        {
            user.MarkEmailConfirmed();
            await context.SaveChangesAsync(cancellationToken);
        }

        // login
        TimeSpan ttl = TimeSpan.FromDays(7);
        AuthSessionRecord sessionRecord = new(user.Id, []);
        string accessToken = await authSessionstore.CreateAsync(
            sessionRecord,
            ttl,
            cancellationToken
        );

        return LoginResponse.Success(accessToken, ttl);
    }
}

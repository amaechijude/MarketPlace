using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using GitgBrand.Api.Infrastructure.OtpValidation;
using MarketPlace.Api.Infrastucture.Email;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed class VerificationCodeHandler(
    HybridCache hybridCache,
    TimeProvider timeProvider,
    Channel<OtpEmailRequest> emailChannel
)
{
    public async ValueTask GenerateAndDispatchOtp(
        Guid userId,
        string email,
        OtpType type,
        string name,
        CancellationToken cancellationToken
    )
    {
        var maxLifeTime = TimeSpan.FromMinutes(10);

        var rawCode = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");

        var value = new OtpVerificationCode(
            userId,
            type,
            timeProvider.GetUtcNow().Add(maxLifeTime)
        );

        await hybridCache.SetAsync(
            key: HashOtp(rawCode),
            value: value,
            options: new HybridCacheEntryOptions
            {
                Expiration = maxLifeTime,
                LocalCacheExpiration = maxLifeTime / 2,
            },
            cancellationToken: cancellationToken
        );

        var emailRequest = CreatOtpEmailRequest(email, rawCode, type, name);

        await emailChannel.Writer.WriteAsync(emailRequest, cancellationToken);
    }

    public async Task<OtpValidationResult> ValidateOtp(
        string userInput,
        OtpType type,
        CancellationToken cancellationToken
    )
    {
        var key = HashOtp(userInput);

        var otp = await hybridCache.GetOrCreateAsync<OtpVerificationCode?>(
            key: key,
            factory: _ => ValueTask.FromResult<OtpVerificationCode?>(null),
            cancellationToken: cancellationToken
        );

        if (otp is null)
            return OtpValidationResult.Failed();

        var isValid = otp.Type == type && otp.ExpiresOn > timeProvider.GetUtcNow();

        await hybridCache.RemoveAsync(key, cancellationToken);

        return isValid ? OtpValidationResult.Success(otp.UserId) : OtpValidationResult.Failed();
    }

    private static string HashOtp(string input)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(input), hash);
        return Convert.ToHexString(hash);
    }

    private static OtpEmailRequest CreatOtpEmailRequest(
        string email,
        string plainOtp,
        OtpType type,
        string name
    )
    {
        var subject = type switch
        {
            OtpType.Register => "Registration Verification",
            OtpType.ResetPassword => "Password Reset",
            _ => "Unknown",
        };
        return new OtpEmailRequest(
            UserEmail: email,
            PlainOtp: plainOtp,
            Name: name,
            Subject: subject,
            Type: type
        );
    }
}

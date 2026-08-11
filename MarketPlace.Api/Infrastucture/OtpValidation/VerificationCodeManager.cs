using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using MarketPlace.Api.Common.Extensions;
using Microsoft.Extensions.Caching.Hybrid;

namespace MarketPlace.Api.Infrastucture.OtpValidation;

public sealed class VerificationCodeManager(
    HybridCache hybridCache,
    TimeProvider timeProvider,
    Channel<OtpEmailRequest> emailChannel
) : ISingletonMarker
{
    public async ValueTask<Guid> GenerateAndDispatchOtp(
        Guid userId,
        string email,
        OtpType type,
        CancellationToken cancellationToken
    )
    {
        TimeSpan maxLifeTime = TimeSpan.FromMinutes(10);

        string rawCode = RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");

        OtpVerificationCode value = new(
            userId,
            HashOtp(rawCode),
            type,
            timeProvider.GetUtcNow().Add(maxLifeTime)
        );

        Guid key = Guid.NewGuid();

        await hybridCache.SetAsync(
            key: key.ToString(),
            value: value,
            options: new HybridCacheEntryOptions
            {
                Expiration = maxLifeTime,
                LocalCacheExpiration = maxLifeTime,
            },
            cancellationToken: cancellationToken
        );

        OtpEmailRequest emailRequest = new(email, type, rawCode);

        await emailChannel.Writer.WriteAsync(emailRequest, cancellationToken);

        return key;
    }

    public async ValueTask<OtpValidationResult> ValidateOtp(
        string userInput,
        Guid otpKey,
        OtpType type,
        CancellationToken cancellationToken
    )
    {
        OtpVerificationCode? otp = await hybridCache.GetOrCreateAsync(
            key: otpKey.ToString(),
            factory: _ => ValueTask.FromResult<OtpVerificationCode?>(null),
            cancellationToken: cancellationToken
        );

        if (otp is null)
            return OtpValidationResult.Failed();

        bool isValidHash = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(HashOtp(userInput)),
            Encoding.UTF8.GetBytes(otp.HashValue)
        );

        bool istype = otp.Type == type;

        return istype && isValidHash
            ? OtpValidationResult.Success(otp.UserId)
            : OtpValidationResult.Failed();
    }

    private static string HashOtp(string input)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(input), hash);
        return Convert.ToHexString(hash);
    }

    private static bool IsValidHa(
        string input,
        OtpVerificationCode otp,
        OtpType type,
        DateTimeOffset utc
    ) =>
        otp.Type == type
        && utc > otp.ExpiresOn
        && CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(HashOtp(input)),
            Encoding.UTF8.GetBytes(otp.HashValue)
        );
}

using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.OtpService;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordHandler(AppDbContext context, OtpEmailHandler otpservice)
    : IScopedRequestHandler
{
    public async Task<ApiResponse<HasOtpResponse>> HandleAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = EmailNormalizer.Normalize(request.Email);
        var user = await context
            .Users.Where(u => u.NormalizedEmail == email)
            .Select(s => new { s.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return ApiResponse<HasOtpResponse>.NotFound("User not found");

        await otpservice.GenerateAndEnqueueAsync(
            user.Id,
            email,
            OtpType.ResetPassword,
            cancellationToken
        );

        return ApiResponse<HasOtpResponse>.Success(new HasOtpResponse());
    }
}

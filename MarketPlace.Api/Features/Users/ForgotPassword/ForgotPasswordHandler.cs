using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Infrastucture.OtpValidation;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Users.ForgotPassword;

public sealed class ForgotPasswordHandler(
    AppDbContext context,
    VerificationCodeManager verificationCodeManager
) : IRequestHandler
{
    public async Task<ApiResponse<ForgotPasswordResponse>> HandleAsync(
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
            return ApiResponse<ForgotPasswordResponse>.NotFound("User not found");

        var otpId = await verificationCodeManager.GenerateAndDispatchOtp(
            user.Id,
            email,
            OtpType.ResetPassword,
            cancellationToken
        );

        return ApiResponse<ForgotPasswordResponse>.Success(new ForgotPasswordResponse(otpId));
    }
}

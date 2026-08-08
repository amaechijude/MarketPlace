using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Infrastucture.OtpValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Users.Register;

public sealed class RegisterUserHandler(
    AppDbContext context,
    VerificationCodeManager verificationCodeManager,
    TimeProvider timeProvider,
    IPasswordHasher<User> hasher
) : IRequestHandler
{
    public async Task<ApiResponse<RegisterUserResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = EmailNormalizer.Normalize(request.Email);
        var user = await context
            .Users.Where(u => u.NormalizedEmail == email)
            .Select(s => new { s.Id, s.EmailConfrimed })
            .FirstOrDefaultAsync(cancellationToken);

        var otpId = Guid.Empty;
        if (user is not null)
        {
            if (user.EmailConfrimed)
            {
                return ApiResponse<RegisterUserResponse>.Success(
                    new RegisterUserResponse(otpId, "Login")
                );
            }
            otpId = await DispatchOtpAsync(user.Id, email, cancellationToken);

            return ApiResponse<RegisterUserResponse>.Success(new RegisterUserResponse(otpId));
        }

        var user1 = User.Create(email, timeProvider.GetUtcNow());
        user1.AddPasswordHash(hasher.HashPassword(user1, request.Password));

        context.Users.Add(user1);
        await context.SaveChangesAsync(cancellationToken);

        otpId = await DispatchOtpAsync(user1.Id, email, cancellationToken);
        return ApiResponse<RegisterUserResponse>.Success(new RegisterUserResponse(otpId));
    }

    private async ValueTask<Guid> DispatchOtpAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken
    ) =>
        await verificationCodeManager.GenerateAndDispatchOtp(
            userId,
            email,
            OtpType.Register,
            cancellationToken
        );
}

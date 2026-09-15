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
) : IScopedRequestHandler
{
    public async Task<ApiResponse<RegisterUserResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = EmailNormalizer.Normalize(request.Email);

        var otpId = Guid.Empty;

        var user = User.Create(email, timeProvider.GetUtcNow());
        user.AddPasswordHash(hasher.HashPassword(user, request.Password));

        context.Users.Add(user);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ApiResponse<RegisterUserResponse>.Success(
                new RegisterUserResponse(otpId, "Login")
            );
        }

        otpId = await DispatchOtpAsync(user.Id, email, cancellationToken);
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

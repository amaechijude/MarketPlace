using MarketPlace.Api.Common.ApiResponseFactory;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Common.Normalizer;
using MarketPlace.Api.Domain.DatabaseContext;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Infrastucture.OtpService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Api.Features.Auth.Register;

public sealed class RegisterUserHandler(
    AppDbContext context,
    OtpEmailHandler otpEmailService,
    TimeProvider timeProvider,
    IPasswordHasher<User> hasher
) : IScopedRequestHandler
{
    public async Task<ApiResponse<HasOtpResponse>> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken
    )
    {
        var email = EmailNormalizer.Normalize(request.Email);

        if (
            await context.Users.AnyAsync(
                u => u.NormalizedEmail == email,
                cancellationToken: cancellationToken
            )
        )
            return ApiResponse<HasOtpResponse>.Success(new HasOtpResponse("Login"));

        var user = User.Create(email, timeProvider.GetUtcNow());
        user.AddPasswordHash(hasher.HashPassword(user, request.Password));

        context.Users.Add(user);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return ApiResponse<HasOtpResponse>.Success(new HasOtpResponse("Login"));
        }

        await otpEmailService.GenerateAndEnqueueAsync(
            user.Id,
            email,
            OtpType.Register,
            cancellationToken
        );

        return ApiResponse<HasOtpResponse>.Success(new HasOtpResponse());
    }
}

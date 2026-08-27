using MarketPlace.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MarketPlace.Api.Infrastucture.Auth;

public static class AuthServiceCollection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
    {
        services
            .AddSingleton<IAuthSessionStore, HybridCacheSessionstore>() // swap for redis
            .AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddAuthentication(AuthSessionOptions.DefaultAuthenticationScheme)
            .AddScheme<AuthSessionOptions, AuthSessionHandler>(
                AuthSessionOptions.DefaultAuthenticationScheme,
                _ => { }
            );
        return services;
    }
}

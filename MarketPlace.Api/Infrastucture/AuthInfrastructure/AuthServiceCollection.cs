using MarketPlace.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MarketPlace.Api.Infrastucture.AuthInfrastructure;

public static class AuthServiceCollection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services)
    {
        services
            .AddSingleton<IAuthSessionStore, HybridCacheSessionstore>() // swap for redis
            .AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>()
            .AddAuthentication(CustomAuthSchemeOptions.DefaultAuthenticationScheme)
            .AddScheme<CustomAuthSchemeOptions, AuthSessionHandler>(
                CustomAuthSchemeOptions.DefaultAuthenticationScheme,
                _ => { }
            );
        return services;
    }
}

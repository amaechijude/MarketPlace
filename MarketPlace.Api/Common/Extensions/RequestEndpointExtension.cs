using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MarketPlace.Api.Common.Extensions;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IRequestEndpoints
{
    void Map(IEndpointRouteBuilder builder);
}

public static class RequestEndpointExtension
{
    public static IServiceCollection AddRequestEndpoints(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var endpoints = assembly
            .DefinedTypes.Where(t =>
                t.IsAssignableTo(typeof(IRequestEndpoints))
                && t is { IsAbstract: false, IsInterface: false, IsClass: true }
            )
            .Select(s => ServiceDescriptor.Transient(typeof(IRequestEndpoints), s));

        services.TryAddEnumerable(endpoints);
        return services;
    }

    public static void MapRequestEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IRequestEndpoints>>().Reverse();

        foreach (var endpoint in endpoints)
        {
            endpoint.Map(app);
        }
    }
}

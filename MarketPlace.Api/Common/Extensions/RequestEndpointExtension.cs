using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace MarketPlace.Api.Common.Extensions;

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
        IEnumerable<ServiceDescriptor> endpoints = assembly
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
        IEnumerable<IRequestEndpoints> endpoints = app
            .Services.GetRequiredService<IEnumerable<IRequestEndpoints>>()
            .Reverse();

        foreach (IRequestEndpoints endpoint in endpoints)
        {
            endpoint.Map(app);
        }
    }
}

using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace MarketPlace.Api.Common.Extensions;

public static class ServiceCollectionExtension
{

    public static IServiceCollection AddRequestEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var endpoints = assembly
            .DefinedTypes.Where(t => t.IsAssignableTo(typeof(IRequestEndpoints)) && t is { IsAbstract: false, IsInterface: false, IsClass: true })
            .Select(s => ServiceDescriptor.Transient(typeof(IRequestEndpoints), s));


        services.TryAddEnumerable(endpoints);
        return services;
    }
    public static IServiceCollection AddRequestHandlers(
       this IServiceCollection services,
       Assembly assembly
   )
    {
        var handlerClasses = assembly
            .GetTypes()
            .Where(t =>
                typeof(IRequestHandler).IsAssignableFrom(t)
                && t is { IsInterface: false, IsAbstract: false, IsClass: true }
            );

        foreach (var handler in handlerClasses)
        {
            services.AddScoped(handler);
        }

        return services;
    }

    public static IServiceCollection AddSingletonHandlers(
    this IServiceCollection services,
    Assembly assembly
)
    {
        var handlerClasses = assembly
            .GetTypes()
            .Where(t =>
                typeof(ISingletonMarker).IsAssignableFrom(t)
                && t is { IsInterface: false, IsAbstract: false, IsClass: true }
            );

        foreach (var handler in handlerClasses)
        {
            services.AddSingleton(handler);
        }

        return services;
    }


    public static void MapRequestEndpoints(this WebApplication app, RouteGroupBuilder? routeGroup = null)
    {
        IEndpointRouteBuilder builder = routeGroup is null ? app : routeGroup;

        var endpoints = app.Services.GetRequiredService<IEnumerable<IRequestEndpoints>>().Reverse();

        foreach (var endpoint in endpoints)
        {
            endpoint.Map(builder);
        }

    }
}
public interface IRequestHandler;
public interface IRequestEndpoints
{
    void Map(IEndpointRouteBuilder builder);
}

public interface ISingletonMarker;
using System.Reflection;

namespace MarketPlace.Api.Common.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddScopedRequestHandlers(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var handlerClasses = GetHandlers(assembly, typeof(IRequestHandler));

        if (handlerClasses.Count == 0)
            return services;

        foreach (var handler in handlerClasses)
        {
            services.AddScoped(handler);
        }

        return services;
    }

    public static IServiceCollection AddTransientHandlers(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        List<Type> handlerClasses = GetHandlers(assembly, typeof(ITransientMarker));

        if (handlerClasses.Count == 0)
            return services;

        foreach (Type handler in handlerClasses)
        {
            services.AddTransient(handler);
        }

        return services;
    }

    public static IServiceCollection AddSingletonHandlers(
        this IServiceCollection services,
        Assembly assembly
    )
    {
        var handlerClasses = GetHandlers(assembly, typeof(ISingletonMarker));

        if (handlerClasses.Count == 0)
            return services;

        foreach (var handler in handlerClasses)
        {
            services.AddScoped(handler);
        }

        return services;
    }

    private static List<Type> GetHandlers(Assembly assembly, Type type) =>
        [
            .. assembly
                .GetTypes()
                .Where(t =>
                    type.IsAssignableFrom(t)
                    && t is { IsInterface: false, IsAbstract: false, IsClass: true }
                ),
        ];
}

public interface IRequestHandler;

public interface ITransientMarker;

public interface ISingletonMarker;

using System.Reflection;

namespace MarketPlace.Api.Common.Extensions;

public static class ServiceCollectionExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddScopedRequestHandlers(
        Assembly assembly
    )
        {
            foreach (var handler in GetHandlers(assembly, typeof(IRequestHandler)))
            {
                services.AddScoped(handler);
            }

            return services;
        }

        public IServiceCollection AddTransientHandlers(
            Assembly assembly
        )
        {
            foreach (var handler in GetHandlers(assembly, typeof(ITransientMarker)))
            {
                services.AddTransient(handler);
            }

            return services;
        }

        public IServiceCollection AddSingletonHandlers(
            Assembly assembly
        )
        {
            foreach (var handler in GetHandlers(assembly, typeof(ISingletonMarker)))
            {
                services.AddScoped(handler);
            }

            return services;
        }
    }

    private static IEnumerable<Type> GetHandlers(Assembly assembly, Type type) =>
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

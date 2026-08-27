using System.Reflection;

namespace MarketPlace.Api.Common.Extensions;

public static class TransientHandlersExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddTransientHandlers(
            Assembly assembly
        )
        {
            var type = typeof(ITransientMarker);
            
            var handlers = assembly
                .GetTypes()
                .Where(t =>
                    type.IsAssignableFrom(t)
                    && t is { IsInterface: false, IsAbstract: false, IsClass: true }
                );
            foreach (var handler in handlers)
            {
                services.AddTransient(handler);
            }

            return services;
        }
    }
}


public interface ITransientMarker;
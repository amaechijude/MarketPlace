using System.Reflection;

namespace MarketPlace.Api.Common.Extensions;

public static class SingletonHandlersExtension
{
    extension(IServiceCollection services)
    {

        public IServiceCollection AddSingletonHandlers(
            Assembly assembly
        )
        {
            var type = typeof(ISingletonMarker);
            
            var handlers = assembly
                .GetTypes()
                .Where(t =>
                    type.IsAssignableFrom(t)
                    && t is { IsInterface: false, IsAbstract: false, IsClass: true }
                );
            foreach (var handler in handlers)
            {
                services.AddSingleton(handler);
            }

            return services;
        }
    }

}


public interface ISingletonMarker;
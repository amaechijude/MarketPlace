using System.Reflection;

namespace MarketPlace.Api.Common.Extensions;

public static class RequestHandlersExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddScopedRequestHandlers(Assembly assembly)
        {
            var type = typeof(IScopedRequestHandler);

            var handlers = assembly
                .GetTypes()
                .Where(t =>
                    type.IsAssignableFrom(t)
                    && t is { IsInterface: false, IsAbstract: false, IsClass: true }
                );

            foreach (var handler in handlers)
            {
                services.AddScoped(handler);
            }

            return services;
        }
    }
}

public interface IScopedRequestHandler;
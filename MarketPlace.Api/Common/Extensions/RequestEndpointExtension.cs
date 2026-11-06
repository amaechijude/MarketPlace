using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scalar.AspNetCore;

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
        _ = services.AddOpenApi();

        var endpoints = assembly
            .DefinedTypes.Where(t =>
                t.IsAssignableTo(typeof(IRequestEndpoints))
                && t
                    is {
                        IsAbstract: false,
                        IsInterface: false,
                        IsClass: true,
                        IsGenericTypeDefinition: false
                    }
            )
            .OrderBy(x => x.FullName, StringComparer.Ordinal)
            .Select(s => ServiceDescriptor.Transient(typeof(IRequestEndpoints), s));

        services.TryAddEnumerable(endpoints);
        return services;
    }

    public static void MapRequestEndpoints(this WebApplication app)
    {
        RouteGroupBuilder versionGroup = app.MapGroup("/api/v1");

        var endpoints = app.Services.GetRequiredService<IEnumerable<IRequestEndpoints>>();
        foreach (var endpoint in endpoints)
            endpoint.Map(versionGroup);
    }

    public static void MapOpenApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapGet("/", () => Results.Redirect("/scalar"))
            .ExcludeFromApiReference()
            .ExcludeFromDescription();
    }
}

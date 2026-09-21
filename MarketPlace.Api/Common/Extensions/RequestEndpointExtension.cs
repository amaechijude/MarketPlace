using Asp.Versioning;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

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


        // Api versioning
        services.AddApiVersioning(options =>
        {
            // API versioning by URL segment (api/v1/users)
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1.0);
            options.AssumeDefaultVersionWhenUnspecified = false;
        }).AddApiExplorer(options =>
        {
            // Calling "AddApiExplorer" is required for OpenAPI versioning to work correctly.
            // Without this, the generated OpenAPI documents will not be versioned.

            // GroupNameFormat specifies the format of the API version.
            // Without this, versioning will use the literal group names. In our case, that would be 1.0.
            // For compatibility with the "default" /openapi/v1.json behavior from Microsoft.AspNetCore.OpenApi, we use v'VVV' so we can retrieve it using v1.json.
            // See https://github.com/dotnet/aspnet-api-versioning/wiki/Version-Format#custom-api-version-format-strings for more information about formatting API versions.
            options.GroupNameFormat = "'v'VVV";
        })
        // You must call "AddOpenApi" after "AddApiVersioning" to ensure you use Asp.Versioning's variant.
        // This variant of "AddOpenApi" is required to properly integrate with API versioning and generate versioned OpenAPI documents.
        // You can call an overload of "AddOpenApi" to customize the OpenAPI generation, just like you would with Microsoft.AspNetCore.OpenApi's "AddOpenApi".
        .AddOpenApi();

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
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .HasApiVersion(new ApiVersion(2))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder versionGroup = app
            .MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        var endpoints = app.Services.GetRequiredService<IEnumerable<IRequestEndpoints>>().Reverse();
        foreach (var endpoint in endpoints)
            endpoint.Map(versionGroup);
    }
}

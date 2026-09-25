using System.Reflection;
using Asp.Versioning;
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
        // Api versioning
        _ = services
            .AddApiVersioning(options =>
            {
                options.ApiVersionReader = new UrlSegmentApiVersionReader(); // API versioning by URL segment (api/v1/users)
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
            })
            .AddOpenApi();

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

        RouteGroupBuilder versionGroup = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        var endpoints = app.Services.GetRequiredService<IEnumerable<IRequestEndpoints>>().Reverse();
        foreach (var endpoint in endpoints)
            endpoint.Map(versionGroup);
    }

    public static void MapOpenApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi().WithDocumentPerVersion();

        app.MapScalarApiReference(options =>
        {
            var descriptions = app.DescribeApiVersions();

            for (var i = 0; i < descriptions.Count; i++)
            {
                var description = descriptions[i];

                // isDefault is used to mark the default API version in Scalar.
                // This decides which version is selected by default when users visit the Scalar UI.
                options.AddDocument(
                    description.GroupName,
                    description.GroupName,
                    isDefault: i == 0
                );
            }
        });
    }
}

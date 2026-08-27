using FluentValidation;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Test.SetUp;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;

namespace MarketPlace.Test.IntegrationTest;

public sealed class ServiceCollectionTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly IServiceProvider _serviceProvider = factory.Services;

    [Fact]
    public void Validation_ShouldBeRegistered()
    {
        using var scope = _serviceProvider.CreateScope();

        // 1. Assert FluentValidation model validation registration (e.g. RegisterRequestValidator)
        var registerValidator = scope.ServiceProvider.GetService<IValidator<RegisterUserRequest>>();
        Assert.NotNull(registerValidator);

        var validationOptions = scope.ServiceProvider.GetService<IOptions<ValidationOptions>>();
        Assert.NotNull(validationOptions);
        Assert.NotNull(validationOptions.Value);
    }

    [Fact]
    public void ProblemDetailsS_ShouldBeRegistered()
    {
        using var scope = _serviceProvider.CreateScope();

        // Assert that the ProblemDetails services (from AddProblemDetails) are registered
        var problemDetailsService = scope.ServiceProvider.GetService<IProblemDetailsService>();
        Assert.NotNull(problemDetailsService);
    }

    [Fact]
    public void ExceptionHandler_ShouldBeRegistered()
    {
        using var scope = _serviceProvider.CreateScope();

        var exceptionHandler = scope
            .ServiceProvider.GetServices<IExceptionHandler>()
            .FirstOrDefault(s => s is GlobalExceptionHandler);

        Assert.NotNull(exceptionHandler);
    }

    [Fact]
    public void Scanned_handlers_use_their_declared_lifetimes()
    {
        var services = new ServiceCollection();
        var assembly = typeof(ServiceCollectionTests).Assembly;

        services
            .AddScopedRequestHandlers(assembly)
            .AddTransientHandlers(assembly)
            .AddSingletonHandlers(assembly);

        Assert.Equal(
            ServiceLifetime.Scoped,
            services.Single(descriptor => descriptor.ServiceType == typeof(ScopedHandler)).Lifetime
        );
        Assert.Equal(
            ServiceLifetime.Transient,
            services
                .Single(descriptor => descriptor.ServiceType == typeof(TransientHandler))
                .Lifetime
        );
        Assert.Equal(
            ServiceLifetime.Singleton,
            services
                .Single(descriptor => descriptor.ServiceType == typeof(SingletonHandler))
                .Lifetime
        );
    }

    private sealed class ScopedHandler : IScopedRequestHandler;

    private sealed class TransientHandler : ITransientMarker;

    private sealed class SingletonHandler : ISingletonMarker;
}

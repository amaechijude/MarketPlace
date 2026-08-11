using MarketPlace.Api.Common.ExceptionHandler;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;

namespace MarketPlace.Test.IntegrationTest.SetupTests;

public sealed class ServiceCollectionTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly IServiceProvider _serviceProvider = factory.Services;

    [Fact]
    public void Validation_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();

        var validation = scope.ServiceProvider.GetService<ValidationOptions>();

        var validationOptionsConfig = scope.ServiceProvider.GetService<
            IConfigureOptions<ValidationOptions>
        >();

        var validationOptions = scope.ServiceProvider.GetService<IOptions<ValidationOptions>>();

        // Assert
        Assert.NotNull(validation);
        Assert.NotNull(validationOptions?.Value);
        Assert.NotNull(validationOptionsConfig);
    }

    [Fact]
    public void ProblemDetailsS_ShouldBeRegistered()
    {
        using var scope = _serviceProvider.CreateScope();

        var problemDetailsService = scope.ServiceProvider.GetService<IProblemDetailsService>();
        Assert.NotNull(problemDetailsService);
    }

    [Fact]
    public void ExceptionHandler_ShouldBeRegistered()
    {
        using var scope = _serviceProvider.CreateScope();

        var exceptionHandler = scope
            .ServiceProvider.GetServices<IExceptionHandler>()
            .FirstOrDefault(s => s.GetType().Name == nameof(GlobalExceptionHandler));

        Assert.NotNull(exceptionHandler);
    }
}

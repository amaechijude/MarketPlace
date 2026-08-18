using FluentValidation;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Test.SetUp;
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

        // 2. Assert builder.Services.AddValidation() (from Program.cs line 45) registration
        var validationOptionsConfig = scope.ServiceProvider.GetService<
            IConfigureOptions<ValidationOptions>
        >();
        Assert.NotNull(validationOptionsConfig);

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
}

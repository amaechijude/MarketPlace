using DotNetEnv;
using DotNetEnv.Configuration;
using FluentValidation;
using MarketPlace.Api;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Auth.Login;
using MarketPlace.Api.Features.Auth.Register;
using MarketPlace.Api.Infrastucture.AuthInfrastructure;
using MarketPlace.Api.Infrastucture.Cache;
using MarketPlace.Api.Infrastucture.Database;
using MarketPlace.Api.Infrastucture.Email;
using MarketPlace.Api.Infrastucture.MediaStorage;
using MarketPlace.Api.Infrastucture.OtpService;
using MarketPlace.Api.Infrastucture.PaymentHandlers;
using MarketPlace.Api.Infrastucture.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseDefaultServiceProvider(
    (_, options) =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    }
);

builder.Configuration.AddDotNetEnv(options: LoadOptions.TraversePath());

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSingleton(_ => TimeProvider.System);

// options
builder
    .Services.AddOptions<GoogleSettings>()
    .Bind(builder.Configuration.GetRequiredSection(nameof(GoogleSettings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Validation, exceptions and problemdetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder
    .Services.AddValidation()
    .AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>(includeInternalTypes: true)
    .AddProblemDetails(options =>
        options.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Instance =
                $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Scheme} {ctx.HttpContext.Request.Path}";
            ctx.ProblemDetails.Extensions["timeStamp"] = DateTimeOffset.UtcNow;
        }
    );

// request and endpoints handlers
var assembly = typeof(Program).Assembly;
builder
    .Services.AddRequestEndpoints(assembly)
    .AddScopedRequestHandlers(assembly)
    .AddSingletonHandlers(assembly)
    .AddTransientHandlers(assembly);

// Authz
builder.Services.AddAuthorization();

// infra
builder
    .Services.AddAuthInfrastructure()
    .AddCacheInfrastructure(builder.Configuration) //cache
    .AddDatabaseInfrastructure(builder.Configuration) // db
    .AddRateLimitingInfrastructure(builder.Configuration) // ratelimit
    .AddMediaStorageInfrastructure(builder.Configuration) // r2
    .AddPaymentHandlersInfrastructure(builder.Configuration)
    .AddEmailInfrastructure(builder.Configuration, builder.Environment)
    .AddOtpInfrastructure(builder.Configuration);

// forwadedheaders
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.All
);

//hosted and background service
builder.Services.AddHostedService<StartupCheck>();

var app = builder.Build();
app.MapDefaultEndpoints();

app.UseForwardedHeaders();

app.MapOpenApiDocumentation();

// Middlewares

// 1 Exception HAndling
app.UseExceptionHandler();

app.UseRateLimiter();

// CORS

//  Use Authn
app.UseAuthentication();

//  Authz
app.UseAuthorization();
app.UseMiddleware<RefreshTokenMiddleware>();

// Antiforgery

// Map endpoints
app.MapRequestEndpoints();
app.Run();

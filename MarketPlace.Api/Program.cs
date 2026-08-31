using DotNetEnv;
using DotNetEnv.Configuration;
using FluentValidation;
using MarketPlace.Api;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Api.Features.Webhooks;
using MarketPlace.Api.Infrastucture.Auth;
using MarketPlace.Api.Infrastucture.Cache;
using MarketPlace.Api.Infrastucture.Database;
using MarketPlace.Api.Infrastucture.Email;
using MarketPlace.Api.Infrastucture.MediaStorage;
using MarketPlace.Api.Infrastucture.OtpValidation;
using MarketPlace.Api.Infrastucture.PaymentHandlers;
using MarketPlace.Api.Infrastucture.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;

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
builder.Services.AddOpenApi();

// options
builder
    .Services.AddOptions<GoogleSettings>()
    .Bind(builder.Configuration.GetRequiredSection(nameof(GoogleSettings)))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Validation, exceptions and problemdetails
builder
    .Services.AddValidation()
    .AddSingleton(_ => TimeProvider.System)
    .AddValidatorsFromAssemblyContaining<RegisterUserRequestValidator>(includeInternalTypes: true)
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails(options =>
        options.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Instance =
                $"{ctx.HttpContext.Request.Method} -> {ctx.HttpContext.Request.Path}";
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

// webhook keyed
builder.Services.AddWebHookKeyedDispatchers();

// Authz
builder.Services.AddAuthorization();

// infra
builder
    .Services.AddAuthInfrastructure()
    .AddCacheInfrastructure(builder.Configuration) //cache
    .AddDatabaseInfrastructure(builder.Configuration) // db
    .AddEmailInfrastructure(builder.Environment) // email
    .AddRateLimitingInfrastructure() // ratelimit
    .AddMediaStorageInfrastructure(builder.Configuration) // r2
    .AddPaymentHandlersInfrastructure(builder.Configuration);

//hosted service
builder.Services.AddHostedService<VerificationCodeBackgroundDispatcher>();

// forwadedheaders
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Middlewares

// 1 Exception HAndling
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseHsts();

app.UseRateLimiter();

// CORS

//  Use Authn
app.UseAuthentication();

//  Authz
app.UseAuthorization();
app.UseMiddleware<RefreshTokenMiddleware>();

// Antiforgery



if (app.Environment.IsDevelopment())
    app.MapGet("/", (HttpResponse response) => response.Redirect("/scalar/v1"))
        .ExcludeFromApiReference()
        .ExcludeFromDescription();

// Map endpoints
app.MapRequestEndpoints();

app.Run();

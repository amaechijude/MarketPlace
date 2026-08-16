using System.Reflection;
using DotNetEnv;
using DotNetEnv.Configuration;
using FluentValidation;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Domain.Entities;
using MarketPlace.Api.Features.Users.Login;
using MarketPlace.Api.Features.Users.Register;
using MarketPlace.Api.Infrastucture.Auth;
using MarketPlace.Api.Infrastucture.Cache;
using MarketPlace.Api.Infrastucture.Database;
using MarketPlace.Api.Infrastucture.Email;
using MarketPlace.Api.Infrastucture.MediaStorage;
using MarketPlace.Api.Infrastucture.OtpValidation;
using MarketPlace.Api.Infrastucture.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
            ctx.ProblemDetails.Extensions["timeStamp"] = DateTimeOffset.UtcNow.ToString("R");
        }
    );

// request and endpoints handlers
Assembly assembly = typeof(Program).Assembly;
builder
    .Services.AddRequestEndpoints(assembly)
    .AddScopedRequestHandlers(assembly)
    .AddSingletonHandlers(assembly)
    .AddTransientHandlers(assembly);

// Cache
builder.Services.AddCacheInfrastructure(builder.Configuration);

// Auth
builder
    .Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>()
    .AddAuthentication(AuthSessionOptions.DefaultAuthenticationScheme)
    .AddScheme<AuthSessionOptions, AuthSessionHandler>(
        AuthSessionOptions.DefaultAuthenticationScheme,
        _ => { }
    );

// Authz
builder.Services.AddAuthorization();

// infra
builder
    .Services.AddDatabaseInfrastructure(builder.Configuration)
    .AddEmailInfrastructure(builder.Environment)
    .AddRateLimitingInfrastructure()
    .AddMediaStorageInfrastructure(builder.Configuration);

//hosted service
builder.Services.AddHostedService<VerificationCodeBackgroundDispatcher>();

// forwadedheaders
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Middlewares

// 1 Exception HAndling
app.UseExceptionHandler();

// 2 Https redirection
// app.UseHttpsRedirection();

// 3 Static files

// 4 Cookie policy

// 5 Use Routing

// 5.1 forwaded heades
app.UseForwardedHeaders();

// 5.5 Rate Limiting
app.UseRateLimiter();

// 5.7 CORS

// 6 Use Authn
app.UseAuthentication();

// 7 Authz
app.UseAuthorization();

// 8 Antiforgery

// Map endpoints
app.MapRequestEndpoints();
if (app.Environment.IsDevelopment())
    app.MapGet("/", (HttpResponse request) => request.Redirect("/scalar/v1"))
        .ExcludeFromApiReference()
        .ExcludeFromDescription();

app.Run();

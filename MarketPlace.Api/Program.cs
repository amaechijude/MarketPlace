using DotNetEnv;
using DotNetEnv.Configuration;
using FluentValidation;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Infrastucture.Auth;
using MarketPlace.Api.Infrastucture.Database;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Validation, exceptions and problemdetails
builder
    .Services.AddValidation()
    .AddSingleton(_ => TimeProvider.System)
    .AddValidatorsFromAssemblyContaining<IRequestHandler>()
    .AddExceptionHandler<GlobalExceptionHandler>()
    .AddProblemDetails(options =>
        options.CustomizeProblemDetails = ctx =>
        {
            ctx.ProblemDetails.Instance =
                $"{ctx.HttpContext.Request.Method} -> {ctx.HttpContext.Request.Path}";
            ctx.ProblemDetails.Extensions["timeStamp"] = DateTimeOffset.UtcNow
                .ToString("R");
        }
    );

// request and endpoints handlers
builder.Services.AddRequestHandlers(typeof(Program).Assembly);
builder.Services.AddRequestEndpoints(typeof(Program).Assembly);
builder.Services.AddSingletonHandlers(typeof(Program).Assembly);

// Cache, AUth and Athz
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(2),
    };
});
builder
    .Services
    //.AddSingleton<AuthSessionstore>()
    .AddAuthentication(AuthSessionOptions.DefaultAuthenticationScheme)
    .AddScheme<AuthSessionOptions, AuthSessionHandler>(
        AuthSessionOptions.DefaultAuthenticationScheme,
        _ => { }
    );
builder.Services.AddAuthorization();

// infra
builder.Services.AddDatabaseInfrastructure(builder.Configuration);

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
app.UseHttpsRedirection();

// 3 Static files

// 4 Cookie policy

// 5 Use Routing

// 5.5 Rate Limiting

// 5.7 CORS

// 6 Use Authn
app.UseAuthentication();

// 7 Authz
app.UseAuthorization();

// 8 Antiforgery

// Map endpoints
app.MapRequestEndpoints(app.MapGroup("api/v1"));

app.Run();

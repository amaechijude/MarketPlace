using DotNetEnv;
using DotNetEnv.Configuration;
using FluentValidation;
using MarketPlace.Api;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Auth.Login;
using MarketPlace.Api.Features.Auth.Register;
using MarketPlace.Api.Features.Webhooks;
using MarketPlace.Api.Infrastucture.AuthInfrastructure;
using MarketPlace.Api.Infrastucture.Cache;
using MarketPlace.Api.Infrastucture.Database;
using MarketPlace.Api.Infrastucture.Email;
using MarketPlace.Api.Infrastucture.MediaStorage;
using MarketPlace.Api.Infrastucture.OtpValidation;
using MarketPlace.Api.Infrastucture.PaymentHandlers;
using MarketPlace.Api.Infrastucture.RateLimiting;
using MarketPlace.Api.Infrastucture.RateLimiting.Redis;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
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
//builder.Services.AddOpenApi();

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

// webhook keyed
builder.Services.AddWebHookKeyedDispatchers();

// Authz
builder.Services.AddAuthorization();

// infra
builder
    .Services.AddAuthInfrastructure()
    .AddCacheInfrastructure(builder.Configuration) //cache
    .AddDatabaseInfrastructure(builder.Configuration) // db
    .AddRateLimitingInfrastructure(builder.Configuration) // ratelimit
    .AddEmailInfrastructure(builder.Environment)
    .AddMediaStorageInfrastructure(builder.Configuration) // r2
    .AddPaymentHandlersInfrastructure(builder.Configuration)
    .AddOtpInfrastructure();

//hosted service

// forwadedheaders
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

if (builder.Environment.IsProduction())
    builder.Services.AddHostedService<StartupCheck>();

var app = builder.Build();
app.MapDefaultEndpoints();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().WithDocumentPerVersion();

    // MapScalarApiReference sets up the Scalar UI at /scalar
    // AddDocuments registers all known API versions so Scalar shows a dropdown to switch between them.
    // You can enrich your OpenAPI document with Scalar specific integrations if you wish.
    // To learn more: https://scalar.com/products/api-references/integrations/aspnetcore/openapi-extensions
    app.MapScalarApiReference(options =>
    {
        var descriptions = app.DescribeApiVersions();

        for (var i = 0; i < descriptions.Count; i++)
        {
            var description = descriptions[i];
            var isDefault = i == descriptions.Count - 1;

            // isDefault is used to mark the default API version in Scalar.
            // This decides which version is selected by default when users visit the Scalar UI.
            options.AddDocument(description.GroupName, description.GroupName, isDefault: isDefault);
        }
    });
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
    app.MapGet("/", (HttpResponse response) => response.Redirect("/scalar"))
        .ExcludeFromApiReference()
        .ExcludeFromDescription();

// Map endpoints
app.MapRequestEndpoints();
app.MapGet(
        "/redis/{email}",
        async (
            [FromRoute] string email,
            HttpResponse response,
            [FromKeyedServices(EmailAddresTokenBucketOptions.Key)] ITokenBucketLimiter rate
        ) =>
        {
            var result = await rate.AllowAsync(email);
            if (!result.Allowed)
            {
                response.AttachRetryAfterHeader(result.RetryAfter);
                return Results.Problem(statusCode: StatusCodes.Status429TooManyRequests);
            }
            return Results.Ok(result);
        }
    )
    .WithIpAddressRateLimiter("redis")
    .WithTags("redis");
app.Run();

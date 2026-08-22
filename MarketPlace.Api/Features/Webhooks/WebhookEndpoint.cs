using MarketPlace.Api.Common.Extensions;
using MarketPlace.Api.Features.Webhooks.Paystack;
using Scalar.AspNetCore;

namespace MarketPlace.Api.Features.Webhooks;

public sealed class WebhookEndpoint : IRequestEndpoints
{
    public void Map(IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("webhook")
            .ExcludeFromApiReference()
            .ExcludeFromDescription()
            .AllowAnonymous();

        PaystackEndpoint.Map(group);
    }
}

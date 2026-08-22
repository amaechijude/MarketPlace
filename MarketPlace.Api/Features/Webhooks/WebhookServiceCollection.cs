using MarketPlace.Api.Features.Webhooks.Paystack;

namespace MarketPlace.Api.Features.Webhooks;

public static class WebhookServiceCollection
{
    extension(IServiceCollection service)
    {
        public IServiceCollection AddWebHookKeyedDispatchers() =>
            service.AddKeyedScoped<IPaystackDispatcher, DispatchChargeSuccess>(PaystackEvents.ChargeSuccess);
    }
}

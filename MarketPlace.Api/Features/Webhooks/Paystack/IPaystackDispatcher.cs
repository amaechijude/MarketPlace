using System.Text.Json;

namespace MarketPlace.Api.Features.Webhooks.Paystack;

public interface IPaystackDispatcher
{
    Task HandleAsync(JsonElement payload, CancellationToken cancellationToken);
}

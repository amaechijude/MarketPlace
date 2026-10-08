using MarketPlace.Api.Features.Products.SearchProduct;

namespace MarketPlace.Api.Infrastucture.AI;

public sealed class FakeEmbeddingService : IEmbeddingService
{
    public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken)
    {
        // For demonstration, returning a dummy 1536-dimensional vector
        var vector = new float[1536];
        Array.Fill(vector, 0.1f);
        return Task.FromResult(vector);
    }
}

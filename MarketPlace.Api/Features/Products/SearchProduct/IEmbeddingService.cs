namespace MarketPlace.Api.Features.Products.SearchProduct;

public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken);
}

namespace MarketPlace.Api.Domain.Entities;

public sealed class CartItem
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public int Quantity { get; private set; } = 1;

    public Guid CartId { get; private set; }
    public Cart Cart { get; private set; } = null!;

    public Guid ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public static CartItem Create(Guid cartId, Guid productVariantId, int quantity) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            CartId = cartId,
            ProductVariantId = productVariantId,
            Quantity = Math.Clamp(quantity, 1, 50),
        };
}

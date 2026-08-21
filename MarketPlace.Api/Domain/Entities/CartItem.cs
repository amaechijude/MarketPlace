namespace MarketPlace.Api.Domain.Entities;

public sealed class CartItem
{
    public Guid Id { get; private init; } = Guid.CreateVersion7();

    public int Quantity { get; private set; } = 1;

    public Guid CartId { get; set; }
    public Cart Cart { get; private set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public static CartItem Create(Guid cartId, Guid productId, int quantity) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            CartId = cartId,
            ProductId = productId,
            Quantity = Math.Clamp(quantity, 1, 50),
        };
}

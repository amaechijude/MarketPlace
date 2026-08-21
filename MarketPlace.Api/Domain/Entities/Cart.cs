namespace MarketPlace.Api.Domain.Entities;

public sealed class Cart
{
    public Guid Id { get; private init; }

    // Navigation properties
    public Guid UserId { get; private init; }
    public User User { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = [];

    public static Cart Create(Guid userId) => new() { Id = Guid.CreateVersion7(), UserId = userId };

    public void AddCartItem(Guid productId, int quantity)
    {
        var item = CartItem.Create(this.Id, productId, quantity);
        CartItems.Add(item);
    }

    public void AddCartItem(IEnumerable<(Guid id, int q)> req)
    {
        var items = req.Select(s => CartItem.Create(this.Id, s.id, s.q));
        foreach (var item in items)
        {
            CartItems.Add(item);
        }
    }
}

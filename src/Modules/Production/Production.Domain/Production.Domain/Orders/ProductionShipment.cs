using Production.Domain.Orders.Rules;

namespace Production.Domain.Orders;

/// <summary>«Відвантаження»: part of an item handed to the client on a day.</summary>
public sealed class ProductionShipment
{
    public const int CommentMaxLength = 1000;

    private ProductionShipment() { }

    internal ProductionShipment(int itemId, int quantity, DateOnly shippedOn, string? comment, string actorName,
        DateTimeOffset now)
    {
        if (quantity is <= 0 or > ProductionItem.MaxQuantity)
            throw new ArgumentOutOfRangeException(nameof(quantity));
        ItemId = itemId;
        Quantity = quantity;
        ShippedOn = shippedOn;
        Comment = OrderText.Optional(comment, CommentMaxLength);
        CreatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        CreatedAt = now;
    }

    public int Id { get; private set; }
    public int OrderId { get; private set; }
    public int ItemId { get; private set; }
    public int Quantity { get; private set; }
    public DateOnly ShippedOn { get; private set; }
    public string? Comment { get; private set; }
    public string CreatedByName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
}

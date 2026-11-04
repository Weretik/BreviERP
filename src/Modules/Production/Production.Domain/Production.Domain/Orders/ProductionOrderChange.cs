namespace Production.Domain.Orders;

/// <summary>
/// A line of «Історія змін»: one field of the order or of an item, its value before and after, who and when. Values are
/// display texts, so the history reads the same after the order changes again.
/// </summary>
public sealed class ProductionOrderChange
{
    public const int FieldMaxLength = 80;
    public const int ValueMaxLength = 4000;

    private ProductionOrderChange() { }

    public ProductionOrderChange(int? itemId, string? itemName, string field, string? before, string? after,
        string actorName, DateTimeOffset at)
    {
        ItemId = itemId;
        ItemName = Cut(itemName);
        Field = OrderText.Required(field, FieldMaxLength);
        Before = Cut(before);
        After = Cut(after);
        ActorName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        At = at;
    }

    public long Id { get; private set; }
    public int OrderId { get; private set; }
    public int? ItemId { get; private set; }
    public string? ItemName { get; private set; }
    public string Field { get; private set; } = null!;
    public string? Before { get; private set; }
    public string? After { get; private set; }
    public string ActorName { get; private set; } = null!;
    public DateTimeOffset At { get; private set; }

    private static string? Cut(string? value) =>
        string.IsNullOrEmpty(value) ? null : value.Length <= ValueMaxLength ? value : value[..ValueMaxLength];
}

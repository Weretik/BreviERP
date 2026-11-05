using Production.Domain.Orders;

namespace Production.Domain.Forecast;

/// <summary>
/// The order «Прогноз пошиття» sews in, as the head of the workshop set it: one row. Orders not in the list come after
/// it by order date and number (PRD-D38).
/// </summary>
public sealed class ForecastQueue
{
    public const int SingletonId = 1;
    public const int MaxOrders = 10_000;

    private ForecastQueue() { }

    public ForecastQueue(DateTimeOffset now)
    {
        Id = SingletonId;
        UpdatedAt = now;
    }

    public int Id { get; private set; }
    public List<int> OrderIds { get; private set; } = [];
    public int Version { get; private set; }
    public string? UpdatedByName { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Set(IReadOnlyList<int> orderIds, string actorName, DateTimeOffset now)
    {
        if (orderIds.Count > MaxOrders || orderIds.Distinct().Count() != orderIds.Count)
            throw new ArgumentException("Order ids must be distinct.", nameof(orderIds));
        OrderIds = [.. orderIds];
        UpdatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        UpdatedAt = now;
        Version++;
    }
}

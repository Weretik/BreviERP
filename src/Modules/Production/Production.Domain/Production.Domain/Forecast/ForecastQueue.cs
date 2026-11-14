using BuildingBlocks.Domain.Errors;
using BuildingBlocks.Domain.Results;
using Production.Domain.Orders;
using Production.Domain.Orders.Rules;

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

    public DomainResult Set(IReadOnlyList<int> orderIds, string actorName, DateTimeOffset now)
    {
        if (orderIds.Count > MaxOrders || orderIds.Distinct().Count() != orderIds.Count)
            return Error.Validation("production.forecast_queue",
                $"У черзі до {MaxOrders} завдань, кожне — один раз.", "orderIds");
        OrderIds = [.. orderIds];
        UpdatedByName = OrderText.Required(actorName, ProductionOrder.PersonMaxLength);
        UpdatedAt = now;
        Version++;
        return DomainResult.Success();
    }
}

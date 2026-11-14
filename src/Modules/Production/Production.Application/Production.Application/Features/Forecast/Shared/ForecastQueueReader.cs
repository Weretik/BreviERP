using Production.Application.Abstractions;
using Production.Domain.Forecast;
using Production.Domain.Orders;

namespace Production.Application.Features.Forecast.Shared;

/// <summary>
/// The orders «Прогноз пошиття» sews (PRD-D38): «Нове» orders, first as the saved queue lists them, then the rest by
/// order date and number.
/// </summary>
public sealed class ForecastQueueReader(IProductionDbContext db)
{
    public async Task<(ForecastQueue? Queue, List<ProductionOrder> Orders)> ReadAsync(bool track,
        CancellationToken cancellationToken)
    {
        var queue = track
            ? await db.ForecastQueues.SingleOrDefaultAsync(x => x.Id == ForecastQueue.SingletonId, cancellationToken)
            : await db.ForecastQueues.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == ForecastQueue.SingletonId, cancellationToken);
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.DeletedAt == null && x.Status == ProductionOrderStatus.New)
            .ToListAsync(cancellationToken);
        var rank = (queue?.OrderIds ?? []).Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);
        var sorted = orders
            .OrderBy(x => rank.TryGetValue(x.Id, out var index) ? index : int.MaxValue)
            .ThenBy(x => x.OrderDate).ThenBy(x => x.Number).ToList();
        return (queue, sorted);
    }
}

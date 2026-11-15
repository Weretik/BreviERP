using Production.Application.Abstractions;
using Production.Domain.Orders;

namespace Production.Application.Features.Orders.Shared.Rules;

/// <summary>
/// Order numbers are «MAX + 1» without gaps (database rules 4). Two creates at once may read the same MAX: the unique
/// index refuses the second save, which takes the next number and saves again.
/// </summary>
internal static class OrderNumbers
{
    private const int Attempts = 5;

    /// <summary>The next order number.</summary>
    public static async Task<int> NextAsync(IProductionDbContext db, CancellationToken cancellationToken) =>
        (await db.Orders.MaxAsync(x => (int?)x.Number, cancellationToken) ?? 0) + 1;

    /// <summary>
    /// Adds and saves a new order. When the same request was saved first (a double press), returns that order instead.
    /// </summary>
    public static async Task<ProductionOrder> SaveNewAsync(IProductionDbContext db, ProductionOrder order,
        CancellationToken cancellationToken)
    {
        db.Orders.Add(order);
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                return order;
            }
            catch (DbUpdateException exception) when (exception.IsUniqueViolation() && attempt < Attempts)
            {
                if (order.CreateRequestId is { } requestId
                    && await db.Orders.AsNoTracking().Include(x => x.Items).Include(x => x.Shipments)
                        .FirstOrDefaultAsync(x => x.CreateRequestId == requestId, cancellationToken) is { } saved)
                {
                    db.Orders.Entry(order).Context.ChangeTracker.Clear();
                    return saved;
                }

                order.TakeNumber(await NextAsync(db, cancellationToken));
            }
        }
    }
}

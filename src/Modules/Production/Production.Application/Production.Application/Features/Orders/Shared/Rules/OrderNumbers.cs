using Production.Application.Abstractions;

namespace Production.Application.Features.Orders.Shared.Rules;

internal static class OrderNumbers
{
    /// <summary>The next order number; the unique index stops a rare race of two saves.</summary>
    public static async Task<int> NextAsync(IProductionDbContext db, CancellationToken cancellationToken) =>
        (await db.Orders.MaxAsync(x => (int?)x.Number, cancellationToken) ?? 0) + 1;
}

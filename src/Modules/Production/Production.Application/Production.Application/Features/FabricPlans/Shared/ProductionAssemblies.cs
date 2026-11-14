using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Contracts.FabricPlans;

namespace Production.Application.Features.FabricPlans.Shared;

/// <summary>
/// What every item of an open («Нове», not deleted) order takes besides fabric, by the composition of its Catalog model
/// («Комплектація» of the model card): the lines shared by the model and those of the item's fabric variant. The item's
/// stage does not matter. «Склад тканин» reads it through `Production.Contracts`.
/// </summary>
public sealed class ProductionAssemblies(IProductionDbContext db, IProductionModelReader models)
{
    public async Task<IReadOnlyList<AssemblyItem>> OpenAsync(CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.Status == ProductionOrderStatus.New && x.DeletedAt == null)
            .ToListAsync(cancellationToken);
        if (orders.Count == 0) return [];
        var compositions = await models.ListCompositionsAsync(cancellationToken);
        var byProduct = compositions.GroupBy(x => x.ProductId).ToDictionary(g => g.Key, g => g.First());
        var byCode = compositions.Where(x => x.ModelCode is not null)
            .GroupBy(x => x.ModelCode!, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First(),
                StringComparer.Ordinal);

        return orders.SelectMany(order => order.Items.Select(item =>
            {
                var composition = item.ProductId is { } productId && byProduct.TryGetValue(productId, out var found)
                    ? found
                    : item.ModelCode is { } code ? byCode.GetValueOrDefault(code) : null;
                var lines = composition?.Lines
                    .Where(x => x.Kind != "Fabric" && (x.ArticleId is null || x.ArticleId == item.ArticleId))
                    .Select(x => new AssemblyLine(x.Kind, x.Code, x.Name, x.Quantity, x.Unit, x.Comment))
                    .ToList();
                return new AssemblyItem(order.Id, order.Number, order.ClientName, order.CreatedAt, item.Id,
                    item.ModelName, item.ModelCode, item.Quantity, lines is { Count: > 0 } ? lines : null, item.DueDate);
            }))
            .ToList();
    }
}

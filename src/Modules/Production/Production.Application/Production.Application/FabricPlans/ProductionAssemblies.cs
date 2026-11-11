using Production.Application.Abstractions;
using Production.Domain.Orders;

namespace Production.Application.FabricPlans;

/// <summary>An item of an open order and the composition lines of its model, for «Фурнітура до замовлення».</summary>
/// <param name="Lines">The non-fabric lines of the model and of the item's fabric variant; null when the model has no
/// composition in the Catalog.</param>
public sealed record ProductionAssemblyItem(
    int OrderId,
    int OrderNumber,
    string ClientName,
    DateTimeOffset OrderCreatedAt,
    int ItemId,
    string ModelName,
    string? ModelCode,
    int Quantity,
    IReadOnlyList<ProductionAssemblyLine>? Lines);

/// <param name="Kind">Accessory or OtherCost.</param>
/// <param name="Code">Reference code (FUR-…), or the cost code; null when unknown.</param>
/// <param name="Name">Reference name; null when the reference line is gone.</param>
/// <param name="Quantity">Norm per product.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record ProductionAssemblyLine(string Kind, string? Code, string? Name, decimal Quantity, string Unit,
    string? Comment);

/// <summary>
/// What every item of an open («Нове», not deleted) order takes besides fabric, by the composition of its Catalog model
/// («Комплектація» of the model card): the lines shared by the model and those of the item's fabric variant. The item's
/// stage does not matter. The host passes it to «Склад тканин».
/// </summary>
public sealed class ProductionAssemblies(IProductionDbContext db, IProductionModelReader models)
{
    public async Task<IReadOnlyList<ProductionAssemblyItem>> OpenAsync(CancellationToken cancellationToken)
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
                    .Select(x => new ProductionAssemblyLine(x.Kind, x.Code, x.Name, x.Quantity, x.Unit, x.Comment))
                    .ToList();
                return new ProductionAssemblyItem(order.Id, order.Number, order.ClientName, order.CreatedAt, item.Id,
                    item.ModelName, item.ModelCode, item.Quantity, lines is { Count: > 0 } ? lines : null);
            }))
            .ToList();
    }
}

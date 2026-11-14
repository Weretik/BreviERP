using Production.Application.Abstractions;
using Production.Domain.FabricPlans;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Abstractions.Sales;

namespace Production.Application.Features.Orders.Queries.GetProductionOrderFilterOptions;

public sealed class GetProductionOrderFilterOptionsHandler(IProductionDbContext db, IProductionModelReader models)
    : IQueryHandler<GetProductionOrderFilterOptionsQuery, Result<ProductionOrderFilterOptionsResponse>>
{
    public async ValueTask<Result<ProductionOrderFilterOptionsResponse>> Handle(
        GetProductionOrderFilterOptionsQuery query, CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => x.DeletedAt == null)
            .ToListAsync(cancellationToken);
        var items = orders.SelectMany(x => x.Items).ToList();
        var culture = StringComparer.CurrentCulture;

        var clients = orders.Select(x => x.ClientName).Distinct().Order(culture).ToList();
        var modelsUsed = items.Where(x => x.ModelCode != null)
            .GroupBy(x => x.ModelCode!)
            .Select(g => new ProductionFilterOption(g.Key, g.First().ModelName))
            .OrderBy(x => x.Code, StringComparer.Ordinal).ToList();
        var fabrics = items.Where(x => x.FabricCode != null)
            .GroupBy(x => x.FabricCode!)
            .Select(g => new ProductionFilterOption(g.Key, g.Select(x => x.FabricName).FirstOrDefault(n => n != null) ?? g.Key))
            .OrderBy(x => x.Code, StringComparer.Ordinal).ToList();
        var colors = items.SelectMany(x => x.AdditionalColors.Prepend(x.MainColor))
            .Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim())
            .GroupBy(PlanColor.Key).Select(g => g.First()).Order(culture).ToList();
        var codes = modelsUsed.Select(x => x.Code).ToHashSet(StringComparer.Ordinal);
        var seasons = (await models.ListSewingModelsAsync(cancellationToken))
            .Where(x => x.Season is not null && codes.Contains(x.Code.Trim().ToUpperInvariant()))
            .Select(x => x.Season!).Distinct().Order(culture).ToList();

        return Result.Success(new ProductionOrderFilterOptionsResponse(clients, modelsUsed, fabrics, colors, seasons));
    }
}

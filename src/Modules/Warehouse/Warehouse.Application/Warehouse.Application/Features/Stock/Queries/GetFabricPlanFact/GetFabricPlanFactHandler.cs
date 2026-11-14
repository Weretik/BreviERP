using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricPlanFact;

public sealed class GetFabricPlanFactHandler(
    IWarehouseDbContext db,
    IWarehouseProductionReader production,
    IWarehouseReferenceReader reference,
    ItemPlans plans) : IQueryHandler<GetFabricPlanFactQuery, Result<FabricPlanFactResponse>>
{
    public async ValueTask<Result<FabricPlanFactResponse>> Handle(GetFabricPlanFactQuery query,
        CancellationToken cancellationToken)
    {
        var issues = await db.FabricIssues.AsNoTracking().Include(x => x.Lines).ThenInclude(x => x.Movements)
            .Where(x => x.ReversedAt == null).ToListAsync(cancellationToken);
        var ids = issues.Select(x => x.ItemId).Distinct().ToArray();
        var items = (await production.ItemsAsync(ids, cancellationToken)).ToDictionary(x => x.ItemId);
        var saved = await plans.SavedAsync(ids, cancellationToken);
        var names = (await reference.FabricsAsync(cancellationToken)).Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => FabricAliases.Normalize(x.Code!)).ToDictionary(g => g.Key, g => g.First().Name);

        var evaluated = issues.GroupBy(x => x.ItemId)
            .Where(g => items.ContainsKey(g.Key))
            .Select(g =>
            {
                var entry = FabricPlanFact.Evaluate(items[g.Key], saved.GetValueOrDefault(g.Key), g.ToList(),
                    code => names.GetValueOrDefault(FabricAliases.Normalize(code)));
                return entry with { Item = entry.Item with { IssuedOn = g.Max(x => x.IssuedOn) } };
            })
            .Where(x => Matches(x, query))
            .ToList();
        var counted = evaluated.Where(x => !x.InProgress && InPeriod(x.Item.IssuedOn, query)).ToList();

        var models = counted.GroupBy(x => x.ModelCode ?? x.ModelName, StringComparer.Ordinal)
            .Select(g => new FabricPlanFactModelResponse(g.First().ModelName, g.First().ModelCode,
                FabricPlanFact.Summary(g.ToList()),
                g.OrderBy(x => x.Item.OrderNumber).ThenBy(x => x.Item.ItemId).Select(x => x.Item).ToList()))
            .OrderBy(x => x.ModelName, StringComparer.CurrentCulture).ToList();
        return Result.Success(new FabricPlanFactResponse(models, FabricPlanFact.Summary(counted),
            evaluated.Count(x => x.InProgress)));
    }

    private static bool Matches(FabricPlanFactEntry entry, GetFabricPlanFactQuery query) =>
        Contains(query.Model, entry.ModelName, entry.ModelCode) &&
        Contains(query.Client, entry.Item.ClientName) &&
        (string.IsNullOrWhiteSpace(query.Fabric) || entry.Item.Materials.Any(m =>
            Contains(query.Fabric, m.PlanFabricCode, m.PlanFabricName, m.PlanColor) ||
            m.Used.Any(u => Contains(query.Fabric, u))));

    private static bool InPeriod(DateOnly? day, GetFabricPlanFactQuery query) =>
        (query.From is null || day >= query.From) && (query.To is null || day <= query.To);

    private static bool Contains(string? search, params string?[] values) =>
        string.IsNullOrWhiteSpace(search) ||
        values.Any(x => x is not null && x.Contains(search.Trim(), StringComparison.CurrentCultureIgnoreCase));
}

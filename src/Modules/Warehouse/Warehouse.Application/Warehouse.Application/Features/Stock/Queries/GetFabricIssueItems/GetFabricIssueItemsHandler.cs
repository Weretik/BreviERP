using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricIssueItems;

public sealed class GetFabricIssueItemsHandler(IWarehouseProductionReader production)
    : IQueryHandler<GetFabricIssueItemsQuery, Result<FabricIssueItemsResponse>>
{
    public const int Limit = 30;

    public async ValueTask<Result<FabricIssueItemsResponse>> Handle(GetFabricIssueItemsQuery query,
        CancellationToken cancellationToken)
    {
        var words = (query.Search ?? "").Trim().ToLower(CultureInfo.GetCultureInfo("uk-UA"))
            .Replace("ц-", "", StringComparison.Ordinal)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var items = (await production.OpenItemsAsync(null, cancellationToken))
            .Where(x => words.All(w => Text(x).Contains(w, StringComparison.Ordinal)))
            .OrderByDescending(x => x.OrderNumber).ThenBy(x => x.ItemId)
            .ToList();
        return Result.Success(new FabricIssueItemsResponse(items.Take(Limit).Select(x => new FabricIssueItemResponse(
                x.OrderId, x.OrderNumber, x.ClientName, x.ItemId, x.ModelName, x.FabricCode, x.FabricName, x.Quantity))
            .ToList(), items.Count > Limit));
    }

    private static string Text(WarehousePlanItem x) =>
        $"{x.OrderNumber} {x.ClientName} {x.ModelName} {x.FabricCode} {x.FabricName}"
            .ToLower(CultureInfo.GetCultureInfo("uk-UA"));
}

using Warehouse.Application.Abstractions;
using Warehouse.Application.Access;
using Warehouse.Application.Contracts;

namespace Warehouse.Application.Stock.Issues;

/// <summary>
/// «Видача на завдання»: items of open orders found by order number, customer or model, whatever fabric they plan
/// (TZ §7.1, A14); 30 at most, newest orders first.
/// </summary>
public sealed record GetFabricIssueItemsQuery(WarehouseActor Actor, string? Search)
    : IQuery<Result<FabricIssueItemsResponse>>;

public sealed class GetFabricIssueItemsQueryValidator : AbstractValidator<GetFabricIssueItemsQuery>
{
    public GetFabricIssueItemsQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

public sealed class GetFabricIssueItemsQueryHandler(IWarehouseProductionReader production)
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

using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Shared;

namespace Production.Application.Features.Orders.Queries.GetProductionModelSummary;

/// <summary>
/// «Зведення за моделями»: every order of the list filters (all pages), items grouped by model with the ordered
/// pieces. <paramref name="Filter"/> is the list query; its page is ignored.
/// </summary>
public sealed record GetProductionModelSummaryQuery(GetProductionOrdersQuery Filter)
    : IQuery<Result<ProductionModelSummaryResponse>>;

public sealed class GetProductionModelSummaryValidator : AbstractValidator<GetProductionModelSummaryQuery>
{
    public GetProductionModelSummaryValidator() =>
        RuleFor(x => x.Filter).NotNull().SetValidator(new GetProductionOrdersValidator());
}

public sealed class GetProductionModelSummaryHandler(IProductionDbContext db, IProductionModelReader models)
    : IQueryHandler<GetProductionModelSummaryQuery, Result<ProductionModelSummaryResponse>>
{
    public async ValueTask<Result<ProductionModelSummaryResponse>> Handle(GetProductionModelSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var orders = await ProductionOrderFiltering.FilterAsync(db, models, query.Filter, cancellationToken);
        orders = ProductionOrderFiltering.ByStatus(orders, query.Filter.Status);
        var items = (await orders.AsNoTracking().Include(x => x.Items).ToListAsync(cancellationToken))
            .SelectMany(x => x.Items.Select(item => new { OrderId = x.Id, item.ModelCode, item.ModelName, item.Quantity }))
            .ToList();
        var seasons = (await models.ListSewingModelsAsync(cancellationToken))
            .GroupBy(x => x.Code.Trim().ToUpperInvariant())
            .ToDictionary(x => x.Key, x => x.First().Season, StringComparer.Ordinal);

        var rows = items
            .GroupBy(x => x.ModelCode ?? x.ModelName.Trim())
            .Select(g =>
            {
                var first = g.First();
                return new ProductionModelSummaryRow(first.ModelCode, first.ModelName,
                    first.ModelCode is null ? null : seasons.GetValueOrDefault(first.ModelCode),
                    ModelCategory.Of(first.ModelName), g.Sum(x => x.Quantity), g.Select(x => x.OrderId).Distinct().Count());
            })
            .OrderBy(x => x.ModelName, StringComparer.CurrentCulture)
            .ToList();
        return Result.Success(new ProductionModelSummaryResponse(rows, rows.Sum(x => x.Quantity)));
    }
}

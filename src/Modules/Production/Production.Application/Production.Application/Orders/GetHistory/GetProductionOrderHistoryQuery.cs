using Production.Application.Contracts;

namespace Production.Application.Orders.GetHistory;

/// <summary>«Історія змін», newest first.</summary>
public sealed record GetProductionOrderHistoryQuery(int Id) : IQuery<Result<IReadOnlyList<ProductionOrderChangeResponse>>>;

public sealed class GetProductionOrderHistoryQueryValidator : AbstractValidator<GetProductionOrderHistoryQuery>
{
    public GetProductionOrderHistoryQueryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetProductionOrderHistoryQueryHandler(Abstractions.IProductionDbContext db)
    : IQueryHandler<GetProductionOrderHistoryQuery, Result<IReadOnlyList<ProductionOrderChangeResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductionOrderChangeResponse>>> Handle(
        GetProductionOrderHistoryQuery query, CancellationToken cancellationToken)
    {
        if (!await db.Orders.AnyAsync(x => x.Id == query.Id && x.DeletedAt == null, cancellationToken))
            return Result<IReadOnlyList<ProductionOrderChangeResponse>>.NotFound(OrderMessages.NotFound);
        var rows = await db.OrderChanges.AsNoTracking().Where(x => x.OrderId == query.Id)
            .OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<ProductionOrderChangeResponse>>(rows.Select(x => x.ToResponse()).ToArray());
    }
}

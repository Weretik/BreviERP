using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Orders.Queries.GetProductionOrderHistory;

/// <summary>«Історія змін», newest first.</summary>
public sealed record GetProductionOrderHistoryQuery(int Id) : IQuery<Result<IReadOnlyList<ProductionOrderChangeResponse>>>;

public sealed class GetProductionOrderHistoryValidator : AbstractValidator<GetProductionOrderHistoryQuery>
{
    public GetProductionOrderHistoryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetProductionOrderHistoryHandler(Abstractions.IProductionDbContext db)
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

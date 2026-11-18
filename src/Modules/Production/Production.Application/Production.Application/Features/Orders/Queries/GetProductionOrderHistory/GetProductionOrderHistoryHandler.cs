using BuildingBlocks.Domain.Identifiers.Orders;
using Production.Domain.Orders.ValueObjects;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Staff.Shared;
using Production.Application.Features.Orders.Shared.Responses;

namespace Production.Application.Features.Orders.Queries.GetProductionOrderHistory;

public sealed class GetProductionOrderHistoryHandler(Abstractions.IProductionDbContext db)
    : IQueryHandler<GetProductionOrderHistoryQuery, Result<IReadOnlyList<ProductionOrderChangeResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ProductionOrderChangeResponse>>> Handle(
        GetProductionOrderHistoryQuery query, CancellationToken cancellationToken)
    {
        if (!await db.Orders.AnyAsync(x => x.Id == new ProductionOrderId(query.Id) && x.DeletedAt == null, cancellationToken))
            return Result<IReadOnlyList<ProductionOrderChangeResponse>>.NotFound(OrderMessages.NotFound);
        var rows = await db.OrderChanges.AsNoTracking().Where(x => x.OrderId == new ProductionOrderId(query.Id))
            .OrderByDescending(x => x.Id).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<ProductionOrderChangeResponse>>(rows.Select(x => x.ToResponse()).ToArray());
    }
}

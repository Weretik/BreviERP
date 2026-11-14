using System.Text;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Queries.GetFabricBooking;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;

public sealed class GetHardwareOrderHandler(IWarehouseProductionReader production, TimeProvider clock)
    : IQueryHandler<GetHardwareOrderQuery, Result<HardwareOrderResponse>>
{
    public async ValueTask<Result<HardwareOrderResponse>> Handle(GetHardwareOrderQuery query,
        CancellationToken cancellationToken)
    {
        var items = await production.OpenAssembliesAsync(cancellationToken);
        var (needs, incomplete) = HardwareNeeds.Calculate(items);
        return Result.Success(new HardwareOrderResponse(items.Select(x => x.OrderId).Distinct().Count(), items.Count,
            needs, incomplete, clock.GetUtcNow()));
    }
}

using System.Text;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Queries.GetFabricBooking;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetHardwareOrder;

public sealed class GetHardwareOrderValidator : AbstractValidator<GetHardwareOrderQuery>
{
    public GetHardwareOrderValidator() => RuleFor(x => x.Actor).ValidActor();
}

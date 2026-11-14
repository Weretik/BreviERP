using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricBooking;

public sealed class GetFabricBookingValidator : AbstractValidator<GetFabricBookingQuery>
{
    public GetFabricBookingValidator() => RuleFor(x => x.Actor).ValidActor();
}

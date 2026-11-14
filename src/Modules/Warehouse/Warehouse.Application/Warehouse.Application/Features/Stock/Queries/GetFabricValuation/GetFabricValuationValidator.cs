using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricValuation;

public sealed class GetFabricValuationValidator : AbstractValidator<GetFabricValuationQuery>
{
    public GetFabricValuationValidator() => RuleFor(x => x.Actor).ValidActor();
}

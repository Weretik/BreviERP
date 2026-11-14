using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;

public sealed class GetFabricStockChoicesValidator : AbstractValidator<GetFabricStockChoicesQuery>
{
    public GetFabricStockChoicesValidator() => RuleFor(x => x.Actor).ValidActor();
}

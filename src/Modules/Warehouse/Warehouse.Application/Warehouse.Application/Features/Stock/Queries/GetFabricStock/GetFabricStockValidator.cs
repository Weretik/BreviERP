using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock;

public sealed class GetFabricStockValidator : AbstractValidator<GetFabricStockQuery>
{
    public GetFabricStockValidator() => RuleFor(x => x.Actor).ValidActor();
}

using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovementHistory;

public sealed class GetFabricMovementHistoryValidator : AbstractValidator<GetFabricMovementHistoryQuery>
{
    public GetFabricMovementHistoryValidator() => RuleFor(x => x.MovementId).GreaterThan(0);
}

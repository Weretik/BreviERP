using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Orders.Queries.GetProductionOrderHistory;

public sealed class GetProductionOrderHistoryValidator : AbstractValidator<GetProductionOrderHistoryQuery>
{
    public GetProductionOrderHistoryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

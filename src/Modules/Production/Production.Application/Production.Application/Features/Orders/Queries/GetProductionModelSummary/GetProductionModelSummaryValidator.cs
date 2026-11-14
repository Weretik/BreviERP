using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Shared;

namespace Production.Application.Features.Orders.Queries.GetProductionModelSummary;

public sealed class GetProductionModelSummaryValidator : AbstractValidator<GetProductionModelSummaryQuery>
{
    public GetProductionModelSummaryValidator() =>
        RuleFor(x => x.Filter).NotNull().SetValidator(new GetProductionOrdersValidator());
}

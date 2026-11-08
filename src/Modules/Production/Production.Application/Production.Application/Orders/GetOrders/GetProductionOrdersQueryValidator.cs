using Production.Domain.Orders;

namespace Production.Application.Orders.GetOrders;

public sealed class GetProductionOrdersQueryValidator : AbstractValidator<GetProductionOrdersQuery>
{
    public GetProductionOrdersQueryValidator()
    {
        RuleFor(x => x.Status).Must(x => x is null || Enum.TryParse<ProductionOrderStatus>(x, true, out var s) && Enum.IsDefined(s))
            .WithMessage("Невідомий статус.");
        RuleFor(x => x.Stage).Must(x => x is null || Enum.TryParse<ProductionStage>(x, true, out var s) && Enum.IsDefined(s))
            .WithMessage("Невідомий етап.");
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.Manager).MaximumLength(ProductionOrder.PersonMaxLength);
        RuleFor(x => x.CustomerId).GreaterThan(0).When(x => x.CustomerId is not null);
        RuleFor(x => x.QuoteId).GreaterThan(0).When(x => x.QuoteId is not null);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetProductionOrdersQuery.MaxPageSize);
    }
}

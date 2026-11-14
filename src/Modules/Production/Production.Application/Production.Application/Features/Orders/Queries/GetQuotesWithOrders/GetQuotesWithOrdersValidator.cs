namespace Production.Application.Features.Orders.Queries.GetQuotesWithOrders;

public sealed class GetQuotesWithOrdersValidator : AbstractValidator<GetQuotesWithOrdersQuery>
{
    public GetQuotesWithOrdersValidator() => RuleFor(x => x.QuoteIds).NotNull();
}

namespace Crm.Application.Quotes.CustomSewing;

public sealed class GetQuoteModelCompositionQueryValidator : AbstractValidator<GetQuoteModelCompositionQuery>
{
    public GetQuoteModelCompositionQueryValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}

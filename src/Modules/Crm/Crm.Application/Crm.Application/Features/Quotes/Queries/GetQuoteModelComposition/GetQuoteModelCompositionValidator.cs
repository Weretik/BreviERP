namespace Crm.Application.Features.Quotes.Queries.GetQuoteModelComposition;

public sealed class GetQuoteModelCompositionValidator : AbstractValidator<GetQuoteModelCompositionQuery>
{
    public GetQuoteModelCompositionValidator() => RuleFor(x => x.ProductId).GreaterThan(0);
}

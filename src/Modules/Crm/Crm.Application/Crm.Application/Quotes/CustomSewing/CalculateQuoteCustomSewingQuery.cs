namespace Crm.Application.Quotes.CustomSewing;

/// <summary>«Виріб з нуля»: the price of each fabric with the chosen accessories and operations (R-20). Nothing is saved.</summary>
public sealed record CalculateQuoteCustomSewingQuery(QuoteCustomSewingPriceInput Input)
    : IQuery<Result<QuoteCustomSewingPriceResponse>>;

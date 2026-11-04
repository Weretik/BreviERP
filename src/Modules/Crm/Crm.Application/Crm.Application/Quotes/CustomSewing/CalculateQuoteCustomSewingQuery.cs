namespace Crm.Application.Quotes.CustomSewing;

/// <summary>«Виріб з нуля»: the price of each variant (its fabrics summed) with the shared accessories and operations (R-20, R-26). Nothing is saved.</summary>
public sealed record CalculateQuoteCustomSewingQuery(QuoteCustomSewingPriceInput Input)
    : IQuery<Result<QuoteCustomSewingPriceResponse>>;

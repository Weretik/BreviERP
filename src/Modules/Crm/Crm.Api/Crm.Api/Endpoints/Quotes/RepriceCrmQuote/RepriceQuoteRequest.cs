namespace Crm.Api.Endpoints.Quotes.RepriceCrmQuote;

/// <param name="Apply">False only shows what would change.</param>
public sealed record RepriceQuoteRequest(int Version, bool Apply);

namespace Crm.Api.Quotes.Requests;

/// <param name="Version">The version the editor loaded.</param>
public sealed record QuoteVersionRequest(int Version);

/// <param name="Apply">False only shows what would change.</param>
public sealed record RepriceQuoteRequest(int Version, bool Apply);

public sealed record SetQuoteResponsibleRequest(int Version, int UserId);

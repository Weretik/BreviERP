namespace Crm.Api.Quotes.Requests;

/// <param name="Apply">False only shows what would change.</param>
public sealed record RepriceQuoteRequest(int Version, bool Apply);

public sealed record SetQuoteResponsibleRequest(int Version, int UserId);

/// <param name="CalculationId">A saved «Ціна вишивки» calculation.</param>
/// <remarks>The client sees the one price category saved with the calculation.</remarks>
public sealed record AddQuoteEmbroideryRequest(long CalculationId);

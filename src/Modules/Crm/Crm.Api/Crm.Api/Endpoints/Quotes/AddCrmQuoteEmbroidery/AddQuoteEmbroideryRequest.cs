namespace Crm.Api.Endpoints.Quotes.AddCrmQuoteEmbroidery;

/// <param name="CalculationId">A saved «Ціна вишивки» calculation.</param>
/// <remarks>The client sees the one price category saved with the calculation.</remarks>
public sealed record AddQuoteEmbroideryRequest(long CalculationId);

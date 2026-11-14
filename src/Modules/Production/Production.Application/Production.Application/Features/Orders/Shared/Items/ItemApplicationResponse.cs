namespace Production.Application.Features.Orders.Shared.Items;

/// <param name="EmbroideryNumber">«ВШ-0042», kept even when the calculation was removed.</param>
/// <param name="Embroidery">The calculation as it is now; null when none or it was removed.</param>
public sealed record ItemApplicationResponse(string Kind, string? Place, string? Description, string? Color,
    long? EmbroideryCalculationId, string? EmbroideryNumber, ProductionEmbroideryResponse? Embroidery);

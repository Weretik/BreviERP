namespace Production.Application.Features.Orders.Shared.Validation;

/// <param name="Kind">Embroidery or Print.</param>
/// <param name="EmbroideryCalculationId">A saved «Ціна вишивки» calculation; only for an embroidery.</param>
public sealed record ItemApplicationInput(string Kind, string? Place, string? Description, string? Color,
    long? EmbroideryCalculationId = null);

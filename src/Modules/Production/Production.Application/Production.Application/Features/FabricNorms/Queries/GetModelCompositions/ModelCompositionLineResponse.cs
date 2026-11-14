namespace Production.Application.Features.FabricNorms.Queries.GetModelCompositions;

/// <param name="Kind">Fabric, Accessory or OtherCost.</param>
/// <param name="Article">The sale SKU of a fabric variant; null for a line of the whole model.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record ModelCompositionLineResponse(
    string Kind, string? Article, string? Code, string? Name, decimal Quantity, string Unit, string? Comment);

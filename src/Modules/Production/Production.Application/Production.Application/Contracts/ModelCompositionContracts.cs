namespace Production.Application.Contracts;

/// <summary>
/// A block of «Комплектація моделей»: a Catalog sewing model with its extra layers (lining, insulation, fleece) and
/// the full composition from «Товари». The page edits nothing here; «Змінити комплектацію» opens the model card.
/// </summary>
public sealed record ModelCompositionResponse(
    string ModelCode,
    string ModelName,
    int? ProductId,
    string? Season,
    bool ModelIsActive,
    IReadOnlyList<ModelLayerResponse> Layers,
    IReadOnlyList<ModelCompositionLineResponse> Lines);

/// <param name="Purpose">Підкладка, Утеплювач or Фліс.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record ModelLayerResponse(string Purpose, string? Code, string? Name, decimal Quantity, string Unit);

/// <param name="Kind">Fabric, Accessory or OtherCost.</param>
/// <param name="Article">The sale SKU of a fabric variant; null for a line of the whole model.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record ModelCompositionLineResponse(
    string Kind, string? Article, string? Code, string? Name, decimal Quantity, string Unit, string? Comment);

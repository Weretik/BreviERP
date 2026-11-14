namespace Production.Application.Features.FabricNorms.Queries.GetModelCompositions;

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

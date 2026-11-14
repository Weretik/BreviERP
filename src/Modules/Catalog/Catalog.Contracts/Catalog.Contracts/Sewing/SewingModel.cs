namespace Catalog.Contracts.Sewing;

/// <summary>
/// The sewing card of a model: its fabric variants with the seamstress labour, the shared materials (Kind is
/// Fabric, Accessory or OtherCost) and the operations.
/// </summary>
public sealed record SewingModel(
    int ProductId,
    string? ModelCode,
    IReadOnlyList<SewingModelArticle> Articles,
    IReadOnlyList<SewingModelMaterial> Materials,
    IReadOnlyList<SewingModelOperation> Operations);

public sealed record SewingModelArticle(
    long Id, int SortOrder, bool Active, int? FabricId, string? FabricName, decimal? FabricQuantity,
    decimal? SeamstressCost);

/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record SewingModelMaterial(
    string Kind, int? ReferenceId, string? CostCode, string? Name, decimal Quantity, string Unit, int SortOrder);

public sealed record SewingModelOperation(
    int OperationId, string? Name, decimal Quantity, decimal? ManualMinutes, int SortOrder);

/// <summary>«Комплектація» of a model: every line with its article, for the workshop and the stock.</summary>
public sealed record SewingModelComposition(int ProductId, string? ModelCode, IReadOnlyList<SewingCompositionLine> Lines);

public sealed record SewingCompositionLine(
    string Kind, string? Article, string? Code, string? Name, decimal Quantity, string Unit, string? Comment,
    long? ArticleId);

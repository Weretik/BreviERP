namespace Catalog.Application.Features.Product.Sewing;

public sealed record SewingCompositionReadSnapshot(
    int ProductId,
    long? ArticleId,
    string? ModelCode,
    long Version,
    bool ArticleIsActive,
    IReadOnlyList<SewingMaterialView> Materials,
    IReadOnlyList<SewingOperationView> Operations);

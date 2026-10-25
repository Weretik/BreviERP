namespace Catalog.Application.Features.Product.Sewing;

public sealed record SewingComposition(
    int ProductId,
    long? ArticleId,
    string? ModelCode,
    long Version,
    IReadOnlyList<SewingMaterialView> Materials,
    IReadOnlyList<SewingOperationView> Operations,
    IReadOnlyList<string> Blockers);


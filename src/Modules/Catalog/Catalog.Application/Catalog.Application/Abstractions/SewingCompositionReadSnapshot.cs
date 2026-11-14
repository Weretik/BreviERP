using Catalog.Application.Features.Product.Shared.Sewing;
namespace Catalog.Application.Abstractions;

public sealed record SewingCompositionReadSnapshot(
    int ProductId,
    long? ArticleId,
    string? ModelCode,
    long Version,
    bool ArticleIsActive,
    IReadOnlyList<SewingMaterialView> Materials,
    IReadOnlyList<SewingOperationView> Operations);

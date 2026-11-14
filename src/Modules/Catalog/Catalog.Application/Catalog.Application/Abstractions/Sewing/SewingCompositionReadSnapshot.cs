using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Shared.Sewing.Views;
namespace Catalog.Application.Abstractions.Sewing;

public sealed record SewingCompositionReadSnapshot(
    int ProductId,
    long? ArticleId,
    string? ModelCode,
    long Version,
    bool ArticleIsActive,
    IReadOnlyList<SewingMaterialView> Materials,
    IReadOnlyList<SewingOperationView> Operations);

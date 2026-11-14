using Catalog.Application.Features.Product.Shared.Sewing;

namespace Catalog.Infrastructure.Persistence.Products;

internal sealed record CompositionReadRow(
    string? ModelCode,
    long CompositionVersion,
    bool ArticleIsActive,
    List<SewingMaterialView> Materials,
    List<SewingOperationView> Operations);

using Catalog.Application.Features.Product.Sewing;

namespace Catalog.Infrastructure.Repositories;

internal sealed record CompositionReadRow(
    string? ModelCode,
    long CompositionVersion,
    bool ArticleIsActive,
    List<SewingMaterialView> Materials,
    List<SewingOperationView> Operations);

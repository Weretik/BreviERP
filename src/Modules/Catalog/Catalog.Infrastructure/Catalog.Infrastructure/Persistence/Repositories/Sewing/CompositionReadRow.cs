using Catalog.Application.Features.Product.Shared.Sewing;


namespace Catalog.Infrastructure.Persistence.Repositories.Sewing;

internal sealed record CompositionReadRow(
    string? ModelCode,
    long CompositionVersion,
    bool ArticleIsActive,
    List<SewingMaterialView> Materials,
    List<SewingOperationView> Operations);

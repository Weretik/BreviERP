using Catalog.Domain.Products;

namespace Catalog.Domain.Products.Readiness;

public sealed record MaterialReadinessFact(
    CompositionLineId Id,
    SewingMaterialKind Kind);

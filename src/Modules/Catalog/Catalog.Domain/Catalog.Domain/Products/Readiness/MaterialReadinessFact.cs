using Catalog.Domain.Products;
using Catalog.Domain.Products.Sewing.Enums;
using Catalog.Domain.Products.Sewing.ValueObjects;

namespace Catalog.Domain.Products.Readiness;

public sealed record MaterialReadinessFact(
    CompositionLineId Id,
    SewingMaterialKind Kind);

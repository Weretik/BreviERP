using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Readiness;

public sealed record MaterialReadinessFact(
    CompositionLineId Id,
    SewingMaterialKind Kind,
    bool IsRequired,
    bool IsReviewed);

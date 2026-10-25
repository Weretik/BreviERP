using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Readiness;

public sealed record OperationReadinessFact(CompositionLineId Id, bool IsReviewed);

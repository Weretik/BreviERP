using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class SewingOperationLine
{
    public CompositionLineId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public ArticleId? ArticleId { get; private set; }
    public OperationReferenceId OperationId { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal? ManualMinutes { get; private set; }
    public int SortOrder { get; private set; }
    public string? Comment { get; private set; }
    private SewingOperationLine() { }
    public static DomainResult<SewingOperationLine> Create(CompositionLineId id, ProductId productId, ArticleId? articleId,
        OperationReferenceId operationId, decimal quantity, decimal? manualMinutes, int sortOrder,
        string? comment = null)
    {
        if (id.Value < 0) throw new ArgumentException("A line id cannot be negative.", nameof(id));
        if (productId.Value <= 0) throw new ArgumentException("A line needs its product id.", nameof(productId));
        if (operationId.Value <= 0) return ProductErrors.PositiveValueRequired("GarmentPartOperationId");
        if (articleId.HasValue) return ProductErrors.ArticleSpecificOperationsNotSupported();
        if (quantity <= 0 || (manualMinutes.HasValue && manualMinutes <= 0) || sortOrder < 0)
            return ProductErrors.PositiveValueRequired(nameof(Quantity));
        var note = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (note?.Length > SewingMaterialLine.CommentMaxLength)
            return ProductErrors.CommentTooLong(SewingMaterialLine.CommentMaxLength);
        return new SewingOperationLine { Id = id, ProductId = productId, ArticleId = articleId, OperationId = operationId,
            Quantity = quantity, ManualMinutes = manualMinutes, SortOrder = sortOrder,
            Comment = note };
    }
}

using BuildingBlocks.Domain.Exceptions;
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
    public bool IsRequired { get; private set; }
    public bool IsReviewed { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    private SewingOperationLine() { }
    public static SewingOperationLine Create(CompositionLineId id, ProductId productId, ArticleId? articleId,
        OperationReferenceId operationId, decimal quantity, decimal? manualMinutes, int sortOrder, bool isRequired,
        bool isReviewed = false, string? reviewedBy = null, DateTimeOffset? reviewedAt = null)
    {
        if (id.Value < 0 || productId.Value <= 0 || operationId.Value <= 0) throw new DomainException(ProductErrors.IdIsRequired());
        if (articleId.HasValue) throw new DomainException(ProductErrors.ArticleSpecificOperationsNotSupported());
        if (quantity <= 0 || (manualMinutes.HasValue && manualMinutes <= 0) || sortOrder < 0)
            throw new DomainException(ProductErrors.PositiveValueRequired(nameof(Quantity)));
        if (isReviewed && (string.IsNullOrWhiteSpace(reviewedBy) || reviewedBy.Trim().Length > 200 || !reviewedAt.HasValue)) throw new DomainException(ProductErrors.ReviewMetadataRequired());
        return new SewingOperationLine { Id = id, ProductId = productId, ArticleId = articleId, OperationId = operationId,
            Quantity = quantity, ManualMinutes = manualMinutes, SortOrder = sortOrder, IsRequired = true,
            IsReviewed = isReviewed, ReviewedBy = isReviewed ? reviewedBy!.Trim() : null, ReviewedAt = isReviewed ? reviewedAt : null };
    }
}

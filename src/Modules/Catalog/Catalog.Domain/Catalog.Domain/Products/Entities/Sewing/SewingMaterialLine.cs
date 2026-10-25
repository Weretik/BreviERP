using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class SewingMaterialLine
{
    public CompositionLineId Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public ArticleId? ArticleId { get; private set; }
    public SewingMaterialKind Kind { get; private set; }
    public FabricReferenceId? FabricId { get; private set; }
    public AccessoryReferenceId? AccessoryId { get; private set; }
    public string? CostCode { get; private set; }
    public decimal Quantity { get; private set; }
    public SewingMaterialUnit Unit { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsReviewed { get; private set; }
    public string? ReviewedBy { get; private set; }
    public DateTimeOffset? ReviewedAt { get; private set; }
    public string ReferenceKey => Kind switch
    {
        SewingMaterialKind.Fabric => $"F:{FabricId?.Value}",
        SewingMaterialKind.Accessory => $"A:{AccessoryId?.Value}",
        _ => $"C:{CostCode}"
    };

    private SewingMaterialLine() { }
    public static SewingMaterialLine Create(CompositionLineId id, ProductId productId, ArticleId? articleId,
        SewingMaterialKind kind, FabricReferenceId? fabricId, AccessoryReferenceId? accessoryId, string? costCode,
        decimal quantity, SewingMaterialUnit unit, bool isRequired, int sortOrder, bool isReviewed = false,
        string? reviewedBy = null, DateTimeOffset? reviewedAt = null)
    {
        if (id.Value < 0 || productId.Value <= 0)
            throw new DomainException(ProductErrors.IdIsRequired());
        if (quantity <= 0 || !Enum.IsDefined(unit) || sortOrder < 0)
            throw new DomainException(ProductErrors.PositiveValueRequired(nameof(Quantity)));
        var validReference = kind switch
        {
            SewingMaterialKind.Fabric => fabricId is { Value: > 0 } && !accessoryId.HasValue
                && string.IsNullOrWhiteSpace(costCode) && unit == SewingMaterialUnit.Metre,
            SewingMaterialKind.Accessory => accessoryId is { Value: > 0 } && !fabricId.HasValue
                && string.IsNullOrWhiteSpace(costCode)
                && unit is SewingMaterialUnit.Piece or SewingMaterialUnit.Metre,
            SewingMaterialKind.OtherCost => !fabricId.HasValue && !accessoryId.HasValue
                && !string.IsNullOrWhiteSpace(costCode) && costCode.Trim().Length <= 80
                && unit == SewingMaterialUnit.Service,
            _ => false
        };
        if (!validReference) throw new DomainException(ProductErrors.MaterialReferenceInvalid());
        if (isReviewed && (string.IsNullOrWhiteSpace(reviewedBy) || reviewedBy.Trim().Length > 200
            || !reviewedAt.HasValue))
            throw new DomainException(ProductErrors.ReviewMetadataRequired());
        return new SewingMaterialLine
        {
            Id = id,
            ProductId = productId,
            ArticleId = articleId,
            Kind = kind,
            FabricId = fabricId,
            AccessoryId = accessoryId,
            CostCode = string.IsNullOrWhiteSpace(costCode) ? null : costCode.Trim(),
            Quantity = quantity,
            Unit = unit,
            IsRequired = isRequired,
            SortOrder = sortOrder,
            IsReviewed = isReviewed,
            ReviewedBy = isReviewed ? reviewedBy!.Trim() : null,
            ReviewedAt = isReviewed ? reviewedAt : null
        };
    }
}


using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.Sewing.Enums;
using Catalog.Domain.Products.Sewing.ValueObjects;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Sewing.Composition;

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
    public int SortOrder { get; private set; }
    public string? Comment { get; private set; }
    public const int CommentMaxLength = 500;
    public string ReferenceKey => Kind switch
    {
        SewingMaterialKind.Fabric => $"F:{FabricId?.Value}",
        SewingMaterialKind.Accessory => $"A:{AccessoryId?.Value}",
        _ => $"C:{CostCode}"
    };

    private SewingMaterialLine() { }
    public static DomainResult<SewingMaterialLine> Create(CompositionLineId id, ProductId productId, ArticleId? articleId,
        SewingMaterialKind kind, FabricReferenceId? fabricId, AccessoryReferenceId? accessoryId, string? costCode,
        decimal quantity, SewingMaterialUnit unit, int sortOrder, string? comment = null)
    {
        if (id.Value < 0) throw new ArgumentException("A line id cannot be negative.", nameof(id));
        if (productId.Value <= 0) throw new ArgumentException("A line needs its product id.", nameof(productId));
        if (quantity <= 0 || !Enum.IsDefined(unit) || sortOrder < 0)
            return ProductErrors.PositiveValueRequired(nameof(Quantity));
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
        if (!validReference) return ProductErrors.MaterialReferenceInvalid();
        var note = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (note?.Length > CommentMaxLength)
            return ProductErrors.CommentTooLong(CommentMaxLength);
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
            SortOrder = sortOrder,
            Comment = note
        };
    }
}

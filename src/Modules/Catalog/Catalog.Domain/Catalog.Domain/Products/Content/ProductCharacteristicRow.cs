
namespace Catalog.Domain.Products;

public sealed class ProductCharacteristicRow
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public string LabelUk { get; private set; } = null!;
    public string LabelRu { get; private set; } = null!;
    public string ValueUk { get; private set; } = null!;
    public string ValueRu { get; private set; } = null!;
    public int SortOrder { get; private set; }

    private ProductCharacteristicRow() { }

    internal static DomainResult<ProductCharacteristicRow> Create(Guid tableId, string labelUk, string labelRu,
        string valueUk, string valueRu, int sortOrder)
    {
        if (tableId == Guid.Empty) throw new ArgumentException("A row needs its table id.", nameof(tableId));
        var label = ProductContentText.Required(labelUk, 200, nameof(LabelUk));
        if (label.IsFailure) return label.Error!;
        var labelRuText = ProductContentText.Required(labelRu, 200, nameof(LabelRu));
        if (labelRuText.IsFailure) return labelRuText.Error!;
        var value = ProductContentText.Required(valueUk, 1_000, nameof(ValueUk));
        if (value.IsFailure) return value.Error!;
        var valueRuText = ProductContentText.Required(valueRu, 1_000, nameof(ValueRu));
        if (valueRuText.IsFailure) return valueRuText.Error!;
        if (sortOrder < 0) return ProductErrors.PositiveValueRequired(nameof(SortOrder));
        return new ProductCharacteristicRow
        {
            Id = Guid.NewGuid(),
            TableId = tableId,
            LabelUk = label.Value,
            LabelRu = labelRuText.Value,
            ValueUk = value.Value,
            ValueRu = valueRuText.Value,
            SortOrder = sortOrder,
        };
    }
}

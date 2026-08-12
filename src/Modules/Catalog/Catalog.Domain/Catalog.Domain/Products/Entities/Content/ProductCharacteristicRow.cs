using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products.Entities;

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

    public ProductCharacteristicRow(Guid tableId, string labelUk, string labelRu, string valueUk, string valueRu, int sortOrder)
    {
        if (tableId == Guid.Empty) throw new DomainException(ProductErrors.CharacteristicRowBelongsToAnotherTable());
        Id = Guid.NewGuid();
        TableId = tableId;
        LabelUk = ProductContentText.Required(labelUk, 200, nameof(LabelUk));
        LabelRu = ProductContentText.Required(labelRu, 200, nameof(LabelRu));
        ValueUk = ProductContentText.Required(valueUk, 1_000, nameof(ValueUk));
        ValueRu = ProductContentText.Required(valueRu, 1_000, nameof(ValueRu));
        if (sortOrder < 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(SortOrder)));
        SortOrder = sortOrder;
    }
}

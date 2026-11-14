
namespace Catalog.Domain.Products;

public sealed class ProductCharacteristicTable
{
    private readonly List<ProductCharacteristicRow> _rows = [];
    public Guid Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public string TitleUk { get; private set; } = null!;
    public string TitleRu { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public IReadOnlyCollection<ProductCharacteristicRow> Rows => _rows.AsReadOnly();

    private ProductCharacteristicTable() { }

    /// <summary>A new table without rows; <see cref="AddRow"/> adds them.</summary>
    public static DomainResult<ProductCharacteristicTable> Create(ProductId productId, string titleUk, string titleRu,
        int sortOrder)
    {
        if (productId.Value == default) throw new ArgumentException("A table needs its product id.", nameof(productId));
        var title = ProductContentText.Required(titleUk, 200, nameof(TitleUk));
        if (title.IsFailure) return title.Error!;
        var titleRuText = ProductContentText.Required(titleRu, 200, nameof(TitleRu));
        if (titleRuText.IsFailure) return titleRuText.Error!;
        if (sortOrder < 0) return ProductErrors.PositiveValueRequired(nameof(SortOrder));
        return new ProductCharacteristicTable
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            TitleUk = title.Value,
            TitleRu = titleRuText.Value,
            SortOrder = sortOrder,
        };
    }

    public DomainResult AddRow(string labelUk, string labelRu, string valueUk, string valueRu, int sortOrder)
    {
        var row = ProductCharacteristicRow.Create(Id, labelUk, labelRu, valueUk, valueRu, sortOrder);
        if (row.IsFailure) return row.Error!;
        _rows.Add(row.Value);
        return DomainResult.Success();
    }

}

using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

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

    public ProductCharacteristicTable(ProductId productId, string titleUk, string titleRu, int sortOrder, IEnumerable<ProductCharacteristicRow> rows)
    {
        if (productId.Value == default) throw new DomainException(ProductErrors.IdIsRequired());
        ArgumentNullException.ThrowIfNull(rows);
        Id = Guid.NewGuid();
        ProductId = productId;
        TitleUk = ProductContentText.Required(titleUk, 200, nameof(TitleUk));
        TitleRu = ProductContentText.Required(titleRu, 200, nameof(TitleRu));
        if (sortOrder < 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(SortOrder)));
        SortOrder = sortOrder;
        var rowList = rows.ToList();
        if (rowList.Any(x => x.TableId != Id)) throw new DomainException(ProductErrors.CharacteristicRowBelongsToAnotherTable());
        _rows.AddRange(rowList);
    }

    public void AddRow(string labelUk, string labelRu, string valueUk, string valueRu, int sortOrder)
        => _rows.Add(new ProductCharacteristicRow(Id, labelUk, labelRu, valueUk, valueRu, sortOrder));

}

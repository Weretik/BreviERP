using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public sealed class ProductInformationBlock
{
    public Guid Id { get; private set; }
    public ProductId ProductId { get; private set; }
    public string TitleUk { get; private set; } = null!;
    public string TitleRu { get; private set; } = null!;
    public string TextUk { get; private set; } = null!;
    public string TextRu { get; private set; } = null!;
    public int SortOrder { get; private set; }

    private ProductInformationBlock() { }

    public ProductInformationBlock(ProductId productId, string titleUk, string titleRu, string textUk, string textRu, int sortOrder)
    {
        if (productId.Value == default) throw new DomainException(ProductErrors.IdIsRequired());
        Id = Guid.NewGuid();
        ProductId = productId;
        TitleUk = ProductContentText.Required(titleUk, 200, nameof(TitleUk));
        TitleRu = ProductContentText.Required(titleRu, 200, nameof(TitleRu));
        TextUk = ProductContentText.Required(textUk, 2_000, nameof(TextUk));
        TextRu = ProductContentText.Required(textRu, 2_000, nameof(TextRu));
        if (sortOrder < 0) throw new DomainException(ProductErrors.PositiveValueRequired(nameof(SortOrder)));
        SortOrder = sortOrder;
    }
}

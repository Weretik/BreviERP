
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products;

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

    public static DomainResult<ProductInformationBlock> Create(ProductId productId, string titleUk, string titleRu,
        string textUk, string textRu, int sortOrder)
    {
        if (productId.Value == default) throw new ArgumentException("A block needs its product id.", nameof(productId));
        var title = ProductContentText.Required(titleUk, 200, nameof(TitleUk));
        if (title.IsFailure) return title.Error!;
        var titleRuText = ProductContentText.Required(titleRu, 200, nameof(TitleRu));
        if (titleRuText.IsFailure) return titleRuText.Error!;
        var text = ProductContentText.Required(textUk, 2_000, nameof(TextUk));
        if (text.IsFailure) return text.Error!;
        var textRuText = ProductContentText.Required(textRu, 2_000, nameof(TextRu));
        if (textRuText.IsFailure) return textRuText.Error!;
        if (sortOrder < 0) return ProductErrors.PositiveValueRequired(nameof(SortOrder));
        return new ProductInformationBlock
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            TitleUk = title.Value,
            TitleRu = titleRuText.Value,
            TextUk = text.Value,
            TextRu = textRuText.Value,
            SortOrder = sortOrder,
        };
    }
}

namespace Content.Domain.ProductPages;

/// <summary>Texts of a product page in one language. Replaced as a whole when the page is saved.</summary>
public sealed class ProductPageText
{
    private ProductPageText() { }

    internal ProductPageText(int productId, string language, ProductPageTextInput input)
    {
        ProductId = productId;
        Language = language;
        Apply(input);
    }

    /// <summary>Takes new texts in place, so the saved row is updated instead of being replaced.</summary>
    internal void Apply(ProductPageTextInput input)
    {
        Name = ProductPageRules.Clean(input.Name);
        SeoTitle = ProductPageRules.Clean(input.SeoTitle);
        SeoDescription = ProductPageRules.Clean(input.SeoDescription);
        Summary = Facts(input.Summary);
        Description = Blocks(input.Description);
        Specs = Groups(input.Specs);
    }

    public int ProductId { get; private set; }

    /// <summary><c>uk</c> or <c>ru</c>.</summary>
    public string Language { get; private set; } = null!;

    public string? Name { get; private set; }
    public string? SeoTitle { get; private set; }
    public string? SeoDescription { get; private set; }
    public IReadOnlyList<ProductFact> Summary { get; private set; } = [];
    public IReadOnlyList<ProductTextBlock> Description { get; private set; } = [];
    public IReadOnlyList<ProductSpecGroup> Specs { get; private set; } = [];

    /// <summary>Nothing the site could show: no name, texts or SEO.</summary>
    public bool IsEmpty => Name is null && SeoTitle is null && SeoDescription is null && Summary.Count == 0
                           && Description.Count == 0 && Specs.Count == 0;

    public bool HasText => Summary.Count > 0 || Description.Count > 0;
    public bool HasSeo => SeoTitle is not null && SeoDescription is not null;

    private static IReadOnlyList<ProductFact> Facts(IReadOnlyList<ProductFact>? facts) =>
        (facts ?? [])
        .Select(x => new ProductFact(ProductPageRules.Clean(x.Label), ProductPageRules.Clean(x.Text) ?? string.Empty))
        .Where(x => x.Text.Length > 0)
        .ToArray();

    private static IReadOnlyList<ProductTextBlock> Blocks(IReadOnlyList<ProductTextBlock>? blocks) =>
        (blocks ?? []).Select(Block).Where(x => x is not null).Select(x => x!).ToArray();

    private static ProductTextBlock? Block(ProductTextBlock block)
    {
        if (block.Kind == ProductTextBlockKind.List)
        {
            var items = (block.Items ?? []).Select(ProductPageRules.Clean).Where(x => x is not null).Select(x => x!).ToArray();
            return items.Length == 0 ? null : new ProductTextBlock(ProductTextBlockKind.List, null, null, items);
        }

        var text = ProductPageRules.Clean(block.Text);
        if (text is null) return null;
        var lead = block.Kind == ProductTextBlockKind.Paragraph ? ProductPageRules.Clean(block.Lead) : null;
        return new ProductTextBlock(block.Kind, text, lead, null);
    }

    private static IReadOnlyList<ProductSpecGroup> Groups(IReadOnlyList<ProductSpecGroup>? groups) =>
        (groups ?? [])
        .Select(group => new ProductSpecGroup(
            ProductPageRules.Clean(group.Title),
            (group.Rows ?? [])
            .Select(row => new ProductSpecRow(ProductPageRules.Clean(row.Name) ?? string.Empty,
                ProductPageRules.Clean(row.Value) ?? string.Empty))
            .Where(row => row.Name.Length > 0 && row.Value.Length > 0)
            .ToArray()))
        .Where(group => group.Rows.Count > 0)
        .ToArray();
}

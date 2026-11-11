namespace Content.Domain.ProductPages;

/// <summary>Cleans text blocks typed by staff; shared by product pages and site categories.</summary>
public static class ProductTextBlocks
{
    /// <summary>Trims every block and drops the empty ones.</summary>
    public static IReadOnlyList<ProductTextBlock> Clean(IReadOnlyList<ProductTextBlock>? blocks) =>
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
}

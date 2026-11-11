using Content.Domain.ProductPages;

namespace Content.Domain.SiteCategories;

/// <summary>Name, SEO and text of a site category in one language. Replaced as a whole when the category is saved.</summary>
public sealed class SiteCategoryText
{
    private SiteCategoryText() { }

    internal SiteCategoryText(string language, SiteCategoryTextInput input)
    {
        Language = language;
        Apply(input);
    }

    /// <summary>Takes new texts in place, so the saved row is updated instead of being replaced.</summary>
    internal void Apply(SiteCategoryTextInput input)
    {
        Name = ProductPageRules.Clean(input.Name);
        SeoTitle = ProductPageRules.Clean(input.SeoTitle);
        SeoDescription = ProductPageRules.Clean(input.SeoDescription);
        Text = ProductTextBlocks.Clean(input.Text);
    }

    public int CategoryId { get; private set; }

    /// <summary><c>uk</c> or <c>ru</c>.</summary>
    public string Language { get; private set; } = null!;

    public string? Name { get; private set; }
    public string? SeoTitle { get; private set; }
    public string? SeoDescription { get; private set; }
    public IReadOnlyList<ProductTextBlock> Text { get; private set; } = [];

    public bool IsEmpty => Name is null && SeoTitle is null && SeoDescription is null && Text.Count == 0;
    public bool HasSeo => SeoTitle is not null && SeoDescription is not null;
}

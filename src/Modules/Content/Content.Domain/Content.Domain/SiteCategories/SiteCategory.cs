using Content.Domain.ProductPages;

namespace Content.Domain.SiteCategories;

/// <summary>
/// A category or subcategory of the public site catalog (CNT-003): its place in the two-level tree, whether the site
/// shows it, its address and its texts in Ukrainian and Russian. The tree belongs to the site, not to admin sections.
/// </summary>
public sealed class SiteCategory
{
    private readonly List<SiteCategoryText> _texts = [];

    private SiteCategory() { }

    /// <summary>A new category, hidden from the site until someone switches it on.</summary>
    public SiteCategory(int? parentId, string slug, int sortOrder, string updatedBy, DateTimeOffset now)
    {
        if (!ProductPageRules.IsValidSlug(slug)) throw new ArgumentException("Invalid slug.", nameof(slug));
        ParentId = parentId;
        Slug = slug;
        SortOrder = sortOrder;
        Touch(updatedBy, now, bump: false);
    }

    public int Id { get; private set; }

    /// <summary>Null for a category; the category id for a subcategory. A subcategory has no children.</summary>
    public int? ParentId { get; private set; }

    public bool ShowOnSite { get; private set; }

    /// <summary>The address part after <c>/catalog/</c>, the same in both languages, unique among categories.</summary>
    public string Slug { get; private set; } = null!;

    /// <summary>Place among the categories of the same parent.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Grows with every change; a save with an older number is refused.</summary>
    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>E-mail of the staff account that made the last change.</summary>
    public string UpdatedBy { get; private set; } = null!;

    public IReadOnlyList<SiteCategoryText> Texts => _texts;

    public SiteCategoryText? TextIn(string language) => _texts.FirstOrDefault(x => x.Language == language);

    public void SetVisibility(bool showOnSite, string updatedBy, DateTimeOffset now)
    {
        if (ShowOnSite == showOnSite) return;
        ShowOnSite = showOnSite;
        Touch(updatedBy, now, bump: true);
    }

    /// <summary>A new place among the categories of the same parent; not a change of the content, so no new revision.</summary>
    public void PlaceAt(int sortOrder) => SortOrder = sortOrder;

    /// <summary>Replaces the place in the tree, the address, the mark and all texts. A language without any text is not kept.</summary>
    public void Replace(int? parentId, int sortOrder, string slug, bool showOnSite,
        IReadOnlyDictionary<string, SiteCategoryTextInput> texts, string updatedBy, DateTimeOffset now)
    {
        if (!ProductPageRules.IsValidSlug(slug)) throw new ArgumentException("Invalid slug.", nameof(slug));
        ParentId = parentId;
        SortOrder = sortOrder;
        Slug = slug;
        ShowOnSite = showOnSite;
        foreach (var language in ProductPageRules.Languages)
        {
            var text = TextIn(language);
            if (!texts.TryGetValue(language, out var input))
            {
                if (text is not null) _texts.Remove(text);
                continue;
            }

            if (text is null) _texts.Add(text = new SiteCategoryText(language, input));
            else text.Apply(input);
            if (text.IsEmpty) _texts.Remove(text);
        }

        Touch(updatedBy, now, bump: true);
    }

    private void Touch(string updatedBy, DateTimeOffset now, bool bump)
    {
        UpdatedBy = updatedBy.Length > ProductPageRules.UpdatedByMaxLength
            ? updatedBy[..ProductPageRules.UpdatedByMaxLength]
            : updatedBy;
        UpdatedAtUtc = now.ToUniversalTime();
        if (bump) Revision++;
    }
}

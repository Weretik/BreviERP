namespace Content.Domain.SiteCategories;

/// <summary>Identity of <see cref="SiteCategory"/>; comparable, so a list sorted by id in memory keeps the database order.</summary>
public readonly record struct SiteCategoryId(int Value) : IComparable<SiteCategoryId>
{
    public int CompareTo(SiteCategoryId other) => Value.CompareTo(other.Value);
}

namespace BuildingBlocks.Domain.Identifiers.Products;

/// <summary>
/// An article (артикул) of a Catalog product. Catalog owns it; Crm and Production keep it on their rows. A not yet saved article has a temporary negative value. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct ArticleId(long Value) : IComparable<ArticleId>
{
    public int CompareTo(ArticleId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static ArticleId? FromNullable(long? value) => value is { } id ? new ArticleId(id) : null;

    public override string ToString() => Value.ToString();
}

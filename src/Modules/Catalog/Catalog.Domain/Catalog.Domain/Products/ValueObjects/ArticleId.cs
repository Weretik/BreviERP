namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct ArticleId
{
    public long Value { get; }
    private ArticleId(long value) => Value = value;
    public static ArticleId Create(long value) => value < 0
        ? throw new ArgumentOutOfRangeException(nameof(value), "Article ID cannot be negative.")
        : new ArticleId(value);
    internal static ArticleId CreateTemporary(long value) => value < 0
        ? new ArticleId(value)
        : throw new ArgumentOutOfRangeException(nameof(value), "Temporary article IDs must be negative.");
    public override string ToString() => Value.ToString();
}

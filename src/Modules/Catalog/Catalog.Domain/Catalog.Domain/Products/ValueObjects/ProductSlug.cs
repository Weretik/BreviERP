using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products.ValueObjects;

public readonly record struct ProductSlug
{
    private const int MaxLength = 160;
    private static readonly Regex SlugRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    private ProductSlug(string value)
    {
        Value = value;
    }

    public static DomainResult<ProductSlug> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ProductErrors.SlugIsRequired();

        var normalizedValue = value.Trim().ToLowerInvariant();
        if (normalizedValue.Length > MaxLength || !SlugRegex.IsMatch(normalizedValue))
            return ProductErrors.SlugFormatInvalid();

        return new ProductSlug(normalizedValue);
    }

    public override string ToString() => Value;

    public static implicit operator string(ProductSlug slug) => slug.Value;
}

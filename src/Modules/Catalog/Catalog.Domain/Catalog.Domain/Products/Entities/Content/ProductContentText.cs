using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products.Entities;

internal static class ProductContentText
{
    public static DomainResult<string> Required(string value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) return ProductErrors.ContentTextRequired();
        var normalized = value.Trim();
        if (normalized.Length > maxLength) return ProductErrors.ContentTextTooLong(maxLength);
        return normalized;
    }
}

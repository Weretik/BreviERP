using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Errors;

namespace Catalog.Domain.Products.Entities;

internal static class ProductContentText
{
    public static string Required(string value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new DomainException(ProductErrors.PositiveValueRequired(field));
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new DomainException(ProductErrors.NameLengthInvalid(maxLength));
        return normalized;
    }
}

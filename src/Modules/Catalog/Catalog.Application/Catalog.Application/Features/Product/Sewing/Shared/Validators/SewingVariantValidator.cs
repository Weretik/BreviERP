namespace Catalog.Application.Features.Product.Sewing;

/// <summary>Field rules shared by creating and changing a fabric variant (CAT-015).</summary>
internal static class SewingVariantValidator
{
    internal static void Validate(List<ValidationError> errors, string? saleSku, int fabricId, decimal fabricQuantity,
        long expectedVersion)
    {
        if (string.IsNullOrWhiteSpace(saleSku) || saleSku.Trim().Length > 80)
            errors.Add(new("saleSku", "Provide a sale SKU of up to 80 characters."));
        if (fabricId <= 0)
            errors.Add(new("fabricId", "Choose a fabric."));
        if (fabricQuantity <= 0 || fabricQuantity > 1_000m || decimal.Round(fabricQuantity, 6) != fabricQuantity)
            errors.Add(new("fabricQuantity", "Use a positive number of metres up to 1000 with at most six decimal places."));
        if (expectedVersion < 0)
            errors.Add(new("expectedVersion", "Version must be nonnegative."));
    }
}

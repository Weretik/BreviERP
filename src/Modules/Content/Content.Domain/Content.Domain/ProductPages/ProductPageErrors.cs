using BuildingBlocks.Domain.Errors;

namespace Content.Domain.ProductPages;

/// <summary>Business rule errors of site pages with an address (products, categories, articles).</summary>
public static class ProductPageErrors
{
    public static Error InvalidSlug() =>
        Error.Validation("content.invalid_slug",
            "Адреса — від 3 до 120 малих латинських літер, цифр і дефісів, без дефіса на початку чи в кінці.", "Slug");
}

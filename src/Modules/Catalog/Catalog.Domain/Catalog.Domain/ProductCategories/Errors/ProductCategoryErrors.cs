using Catalog.Domain.Errors;

namespace Catalog.Domain.ProductCategories.Errors;

public static class ProductCategoryErrors
{
    public static CatalogDomainError IdIsRequired() =>
        new("Catalog.ProductCategory.Id.Required", "Product category id must be provided");

    public static CatalogDomainError NameIsRequired() =>
        new("Catalog.ProductCategory.Name.Required", "Product category name is required");

    public static CatalogDomainError NameLengthInvalid(int maxLength) =>
        new("Catalog.ProductCategory.Name.LengthInvalid", $"Product category name must be {maxLength} characters or less");

    public static CatalogDomainError SlugIsRequired() =>
        new("Catalog.ProductCategory.Slug.Required", "Product category slug is required");

    public static CatalogDomainError SlugFormatInvalid() =>
        new("Catalog.ProductCategory.Slug.FormatInvalid", "Product category slug must contain lowercase letters, numbers, and hyphens only");

    public static CatalogDomainError SortOrderOutOfRange() =>
        new("Catalog.ProductCategory.SortOrder.OutOfRange", "Product category sort order must be zero or greater");

    public static CatalogDomainError ParentCannotBeSelf() =>
        new("Catalog.ProductCategory.Parent.SelfReference", "Product category cannot be its own parent");

    public static CatalogDomainError ParentCannotBeDescendant() =>
        new("Catalog.ProductCategory.Parent.DescendantReference", "Product category cannot be moved under its own descendant");

    public static CatalogDomainError PathIsRequired() =>
        new("Catalog.ProductCategory.Path.Required", "Product category path is required");

    public static CatalogDomainError PathLengthInvalid(int maxLength) =>
        new("Catalog.ProductCategory.Path.LengthInvalid", $"Product category path must be {maxLength} characters or less");

    public static CatalogDomainError PathAncestorMismatch() =>
        new("Catalog.ProductCategory.Path.AncestorMismatch", "Product category path does not belong to the expected ancestor path");
}
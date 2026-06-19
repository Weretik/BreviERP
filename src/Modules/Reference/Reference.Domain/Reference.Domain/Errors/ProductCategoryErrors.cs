namespace Reference.Domain.Errors;

public static class ProductCategoryErrors
{
    public static ReferenceDomainError IdIsRequired() =>
        new("Reference.ProductCategory.Id.Required", "Product category id must be provided");

    public static ReferenceDomainError NameIsRequired() =>
        new("Reference.ProductCategory.Name.Required", "Product category name is required");

    public static ReferenceDomainError NameLengthInvalid(int maxLength) =>
        new("Reference.ProductCategory.Name.LengthInvalid", $"Product category name must be {maxLength} characters or less");

    public static ReferenceDomainError SlugIsRequired() =>
        new("Reference.ProductCategory.Slug.Required", "Product category slug is required");

    public static ReferenceDomainError SlugFormatInvalid() =>
        new("Reference.ProductCategory.Slug.FormatInvalid", "Product category slug must contain lowercase letters, numbers, and hyphens only");

    public static ReferenceDomainError SortOrderOutOfRange() =>
        new("Reference.ProductCategory.SortOrder.OutOfRange", "Product category sort order must be zero or greater");

    public static ReferenceDomainError ParentCannotBeSelf() =>
        new("Reference.ProductCategory.Parent.SelfReference", "Product category cannot be its own parent");

    public static ReferenceDomainError ParentCannotBeDescendant() =>
        new("Reference.ProductCategory.Parent.DescendantReference", "Product category cannot be moved under its own descendant");

    public static ReferenceDomainError PathIsRequired() =>
        new("Reference.ProductCategory.Path.Required", "Product category path is required");

    public static ReferenceDomainError PathLengthInvalid(int maxLength) =>
        new("Reference.ProductCategory.Path.LengthInvalid", $"Product category path must be {maxLength} characters or less");
}

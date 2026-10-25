namespace Catalog.Application.Features.Product.Sewing;

internal static class CreateSewingArticleCommandValidator
{
    internal static List<ValidationError> Validate(CreateSewingArticleCommand command)
    {
        var errors = new List<ValidationError>();
        if (command.ProductId is <= 0 or > 1_000_000_000)
            errors.Add(new("productId", "Invalid Product ID."));
        if (string.IsNullOrWhiteSpace(command.Request.SaleSku) || command.Request.SaleSku.Trim().Length > 80)
            errors.Add(new("saleSku", "Provide a sale SKU of up to 80 characters."));
        if (command.Request.SortOrder < 0)
            errors.Add(new("sortOrder", "Order must be nonnegative."));
        if (command.Request.ExpectedVersion < 0)
            errors.Add(new("expectedVersion", "Version must be nonnegative."));
        return errors;
    }
}

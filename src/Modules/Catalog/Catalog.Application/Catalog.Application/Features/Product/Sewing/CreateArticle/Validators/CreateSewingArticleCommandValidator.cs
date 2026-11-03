namespace Catalog.Application.Features.Product.Sewing;

internal static class CreateSewingArticleCommandValidator
{
    internal static List<ValidationError> Validate(CreateSewingArticleCommand command)
    {
        var errors = new List<ValidationError>();
        var request = command.Request;
        if (command.ProductId is <= 0 or > 1_000_000_000)
            errors.Add(new("productId", "Invalid Product ID."));
        SewingVariantValidator.Validate(errors, request.SaleSku, request.FabricId, request.FabricQuantity,
            request.ExpectedVersion);
        if (request.SortOrder < 0)
            errors.Add(new("sortOrder", "Order must be nonnegative."));
        return errors;
    }
}

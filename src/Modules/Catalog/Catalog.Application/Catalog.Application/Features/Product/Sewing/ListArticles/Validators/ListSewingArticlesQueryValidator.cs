namespace Catalog.Application.Features.Product.Sewing;

internal static class ListSewingArticlesQueryValidator
{
    internal static List<ValidationError> Validate(ListSewingArticlesQuery query)
    {
        var errors = new List<ValidationError>();
        if (query.ProductId is <= 0 or > 1_000_000_000)
            errors.Add(new("productId", "Invalid Product ID."));
        return errors;
    }
}

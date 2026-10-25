namespace Catalog.Application.Features.Product.Sewing;

internal static class GetSewingCompositionQueryValidator
{
    internal static List<ValidationError> Validate(GetSewingCompositionQuery query)
    {
        var errors = new List<ValidationError>();
        if (query.ProductId is <= 0 or > 1_000_000_000 || query.ArticleId is <= 0)
            errors.Add(new("id", "Invalid Product or Article ID."));
        return errors;
    }
}

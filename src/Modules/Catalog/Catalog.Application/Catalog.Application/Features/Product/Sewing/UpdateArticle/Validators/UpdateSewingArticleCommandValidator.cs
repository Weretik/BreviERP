namespace Catalog.Application.Features.Product.Sewing;

internal static class UpdateSewingArticleCommandValidator
{
    internal static List<ValidationError> Validate(UpdateSewingArticleCommand command)
    {
        var errors = new List<ValidationError>();
        if (command.ProductId is <= 0 or > 1_000_000_000 || command.ArticleId <= 0)
            errors.Add(new("id", "Invalid Product or Article ID."));
        var request = command.Request;
        SewingVariantValidator.Validate(errors, request.SaleSku, request.FabricId, request.FabricQuantity,
            request.ExpectedVersion);
        return errors;
    }
}

namespace Catalog.Application.Features.Product.Sewing;

internal static class ReplaceSewingArticleCompositionCommandValidator
{
    internal static List<ValidationError> Validate(ReplaceSewingArticleCompositionCommand command)
    {
        var errors = SewingMaterialInputValidator.Validate(command.Request.Materials);
        if (command.Request.ExpectedVersion < 0)
            errors.Add(new("expectedVersion", "Version must be nonnegative."));
        if (command.ProductId is <= 0 or > 1_000_000_000 || command.ArticleId <= 0)
            errors.Add(new("id", "Invalid Product or Article ID."));
        if (!string.IsNullOrWhiteSpace(command.Actor) && command.Actor.Length > 200)
            errors.Add(new("actor", "Actor must be at most 200 characters."));
        return errors;
    }
}


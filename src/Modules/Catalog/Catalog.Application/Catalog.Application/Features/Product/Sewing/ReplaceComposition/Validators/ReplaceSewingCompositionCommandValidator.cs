namespace Catalog.Application.Features.Product.Sewing;

internal static class ReplaceSewingCompositionCommandValidator
{
    internal static List<ValidationError> Validate(ReplaceSewingCompositionCommand command)
    {
        var request = command.Request;
        var errors = SewingMaterialInputValidator.Validate(request.Materials);
        errors.AddRange(SewingOperationInputValidator.Validate(request.Operations));
        if (request.ExpectedVersion < 0)
            errors.Add(new("expectedVersion", "Version must be nonnegative."));
        if (command.ProductId is <= 0 or > 1_000_000_000)
            errors.Add(new("productId", "Invalid Product ID."));
        if (request.ModelCode?.Trim().Length > 80)
            errors.Add(new("modelCode", "Model code must be at most 80 characters."));
        if (request.Operations is null)
            errors.Add(new("operations", "Operations are required."));
        if (!string.IsNullOrWhiteSpace(command.Actor) && command.Actor.Length > 200)
            errors.Add(new("actor", "Actor must be at most 200 characters."));
        return errors;
    }
}


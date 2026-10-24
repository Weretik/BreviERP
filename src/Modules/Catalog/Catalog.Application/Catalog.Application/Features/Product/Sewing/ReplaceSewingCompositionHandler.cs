using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Contracts.Reference;

namespace Catalog.Application.Features.Product.Sewing;

public sealed class ReplaceSewingCompositionHandler(ISewingCompositionStore store, IProductReferenceReader referenceReader)
    : ICommandHandler<ReplaceSewingCompositionCommand, Result<SewingComposition>>
{
    public async ValueTask<Result<SewingComposition>> Handle(ReplaceSewingCompositionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var errors = CompositionInputValidator.Validate(request.ExpectedVersion, request.Materials, request.Operations);
        if (command.ProductId is <= 0 or > 1_000_000_000) errors.Add(new("productId", "Invalid Product ID."));
        if (request.ModelCode?.Trim().Length > 80) errors.Add(new("modelCode", "Model code must be at most 80 characters."));
        if (request.Operations is null) return Result.Invalid(new ValidationError("operations", "Operations are required."));
        if (string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200) return Result.Unauthorized();
        if (errors.Count > 0) return Result.Invalid(errors);
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        if (product is null) return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion) return Result.Conflict("Composition changed. Reload before saving.");
        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var missing = CompositionReferenceValidation.MissingReference(request.Materials, request.Operations, references);
        if (missing is not null) return Result.NotFound(missing);
        var now = TimeProvider.System.GetUtcNow();
        try
        {
            var materials = request.Materials.Select(x => CompositionLineFactory.Material(x, product.Id, null, command.Actor, now)).ToArray();
            var operations = request.Operations.Select(x => CompositionLineFactory.Operation(x, product.Id, command.Actor, now)).ToArray();
            // Validate the combined effective set before changing the model code or any stored lines.
            product.ReplaceComposition(product.MaterialLines.Where(x => x.ArticleId.HasValue).Concat(materials), operations, now);
            product.SetModelCode(request.ModelCode, now);
        }
        catch (DomainException exception) { return Result.Conflict(exception.Message); }
        if (!await store.SaveAsync(cancellationToken)) return Result.Conflict("Composition or model code changed. Reload before saving.");
        return Result.Success((await store.ReadCompositionAsync(command.ProductId, null, cancellationToken))!);
    }
}

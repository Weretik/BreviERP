using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Contracts.Reference;

namespace Catalog.Application.Features.Product.Sewing;

public sealed class ReplaceSewingCompositionHandler(
    ISewingCompositionStore store,
    ISewingCompositionReadStore readStore,
    IProductReferenceReader referenceReader)
    : ICommandHandler<ReplaceSewingCompositionCommand, Result<SewingComposition>>
{
    public async ValueTask<Result<SewingComposition>> Handle(ReplaceSewingCompositionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var errors = ReplaceSewingCompositionCommandValidator.Validate(command);
        if (string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200)
            return Result.Unauthorized();
        if (errors.Count > 0)
            return Result.Invalid(errors);
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion)
            return Result.Conflict("Composition changed. Reload before saving.");
        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var missing = CompositionReferenceValidation.MissingReference(request.Materials, request.Operations, references);
        if (missing is not null)
            return Result.NotFound(missing);
        var now = TimeProvider.System.GetUtcNow();
        try
        {
            var materials = request.Materials
                .Select(line => CompositionLineFactory.Material(line, product.Id, null, command.Actor, now))
                .ToArray();
            var operations = request.Operations
                .Select(line => CompositionLineFactory.Operation(line, product.Id, command.Actor, now))
                .ToArray();
            // Validate the combined effective set before changing the model code or any stored lines.
            product.ReplaceComposition(product.MaterialLines.Where(line => line.ArticleId.HasValue)
                .Concat(materials), operations, now);
            product.SetModelCode(request.ModelCode, now);
        }
        catch (DomainException exception)
        {
            return Result.Conflict(exception.Message);
        }
        if (!await store.SaveAsync(cancellationToken))
            return Result.Conflict("Composition or model code changed. Reload before saving.");
        var snapshot = await readStore.ReadCompositionAsync(command.ProductId, null, cancellationToken);
        return Result.Success(SewingCompositionReadAssembler.Assemble(snapshot!));
    }
}

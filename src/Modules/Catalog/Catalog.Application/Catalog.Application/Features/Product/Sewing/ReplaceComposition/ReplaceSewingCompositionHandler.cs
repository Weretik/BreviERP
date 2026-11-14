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
        if (string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200)
            return Result.Unauthorized();
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion)
            return Result.Conflict("Склад уже змінили. Оновіть сторінку перед збереженням.");
        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var missing = CompositionReferenceValidation.MissingReference(request.Materials, request.Operations, references);
        if (missing is not null)
            return Result.NotFound(missing);
        var now = TimeProvider.System.GetUtcNow();
        var materials = CompositionLineFactory.Materials(request.Materials, product.Id);
        if (materials.IsFailure) return materials.Error!.ToResult<SewingComposition>();
        var operations = CompositionLineFactory.Operations(request.Operations, product.Id);
        if (operations.IsFailure) return operations.Error!.ToResult<SewingComposition>();
        // Validate the combined effective set before changing the model code or any stored lines.
        var replaced = product.ReplaceComposition(product.MaterialLines.Where(line => line.ArticleId.HasValue)
            .Concat(materials.Value), operations.Value, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<SewingComposition>();
        var coded = product.SetModelCode(request.ModelCode, now);
        if (coded.IsFailure) return coded.Error!.ToResult<SewingComposition>();
        if (!await store.SaveAsync(cancellationToken))
            return Result.Conflict("Склад або код моделі вже змінили. Оновіть сторінку перед збереженням.");
        var snapshot = await readStore.ReadCompositionAsync(command.ProductId, null, cancellationToken);
        return Result.Success(SewingCompositionReadAssembler.Assemble(snapshot!));
    }
}

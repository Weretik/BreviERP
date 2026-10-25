using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Contracts.Reference;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Sewing;

public sealed class ReplaceSewingArticleCompositionHandler(
    ISewingCompositionStore store,
    ISewingCompositionReadStore readStore,
    IProductReferenceReader referenceReader)
    : ICommandHandler<ReplaceSewingArticleCompositionCommand, Result<SewingComposition>>
{
    public async ValueTask<Result<SewingComposition>> Handle(ReplaceSewingArticleCompositionCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var errors = ReplaceSewingArticleCompositionCommandValidator.Validate(command);
        if (string.IsNullOrWhiteSpace(command.Actor) || command.Actor.Length > 200)
            return Result.Unauthorized();
        if (errors.Count > 0)
            return Result.Invalid(errors);
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        if (product is null || !product.Articles.Any(article => article.Id.Value == command.ArticleId))
            return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion)
            return Result.Conflict("Composition changed. Reload before saving.");
        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var missing = CompositionReferenceValidation.MissingReference(request.Materials, [], references);
        if (missing is not null)
            return Result.NotFound(missing);
        var now = TimeProvider.System.GetUtcNow();
        try
        {
            var articleId = ArticleId.Create(command.ArticleId);
            product.ReplaceArticleComposition(articleId,
                request.Materials.Select(line => CompositionLineFactory.Material(line, product.Id,
                    articleId, command.Actor, now)), now);
        }
        catch (DomainException exception)
        {
            return Result.Conflict(exception.Message);
        }
        if (!await store.SaveAsync(cancellationToken))
            return Result.Conflict("Composition changed. Reload before saving.");
        var snapshot = await readStore.ReadCompositionAsync(command.ProductId, command.ArticleId, cancellationToken);
        return Result.Success(SewingCompositionReadAssembler.Assemble(snapshot!));
    }
}

using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Sewing;

/// <summary>
/// Removes a fabric variant with its fabric line (CAT-015). A variant whose price was already saved keeps its
/// price history, so it cannot be removed; it can be switched off instead.
/// </summary>
public sealed class DeleteSewingArticleHandler(ISewingCompositionStore store)
    : ICommandHandler<DeleteSewingArticleCommand, Result>
{
    public const string HasPriceHistory = "The variant already has saved prices. Switch it off instead of deleting.";

    public async ValueTask<Result> Handle(DeleteSewingArticleCommand command, CancellationToken cancellationToken)
    {
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        var articleId = ArticleId.Create(command.ArticleId);
        if (product is null || product.Articles.All(x => x.Id != articleId))
            return Result.NotFound();
        if (product.CompositionVersion != command.ExpectedVersion)
            return Result.Conflict("Composition changed. Reload before saving.");
        if (await store.HasPriceHistoryAsync(command.ArticleId, cancellationToken))
            return Result.Conflict(HasPriceHistory);
        try
        {
            product.RemoveArticle(articleId, TimeProvider.System.GetUtcNow());
        }
        catch (DomainException exception)
        {
            return Result.Conflict(exception.Message);
        }
        return await store.SaveAsync(cancellationToken)
            ? Result.Success()
            : Result.Conflict("Composition changed. Reload before saving.");
    }
}

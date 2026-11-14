using Catalog.Domain.Products;
using Catalog.Application.Abstractions;
using Catalog.Domain.Products.Sewing.ValueObjects;

namespace Catalog.Application.Features.Product.Commands.DeleteSewingArticle;

/// <summary>
/// Removes a fabric variant with its fabric line (CAT-015). A variant whose price was already saved keeps its
/// price history, so it cannot be removed; it can be switched off instead.
/// </summary>
public sealed class DeleteSewingArticleHandler(ISewingCompositionStore store)
    : ICommandHandler<DeleteSewingArticleCommand, Result>
{
    public const string HasPriceHistory = "У варіанта вже є збережені ціни. Вимкніть його замість видалення.";

    public async ValueTask<Result> Handle(DeleteSewingArticleCommand command, CancellationToken cancellationToken)
    {
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        var articleId = ArticleId.Create(command.ArticleId);
        if (product is null || product.Articles.All(x => x.Id != articleId))
            return Result.NotFound();
        if (product.CompositionVersion != command.ExpectedVersion)
            return Result.Conflict("Склад уже змінили. Оновіть сторінку перед збереженням.");
        if (await store.HasPriceHistoryAsync(command.ArticleId, cancellationToken))
            return Result.Conflict(HasPriceHistory);
        var removed = product.RemoveArticle(articleId, TimeProvider.System.GetUtcNow());
        if (removed.IsFailure) return removed.Error!.ToResult();
        return await store.SaveAsync(cancellationToken)
            ? Result.Success()
            : Result.Conflict("Склад уже змінили. Оновіть сторінку перед збереженням.");
    }
}

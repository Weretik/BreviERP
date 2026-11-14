using Catalog.Application.Contracts.Reference;
using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Sewing;

/// <summary>Changes the sale SKU, activity and fabric of one variant (CAT-015).</summary>
public sealed class UpdateSewingArticleHandler(ISewingCompositionStore store, IProductReferenceReader references)
    : ICommandHandler<UpdateSewingArticleCommand, Result<SewingArticleView>>
{
    public async ValueTask<Result<SewingArticleView>> Handle(UpdateSewingArticleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        var articleId = ArticleId.Create(command.ArticleId);
        var article = product?.Articles.FirstOrDefault(x => x.Id == articleId);
        if (product is null || article is null)
            return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion)
            return Result.Conflict("Склад уже змінили. Оновіть сторінку перед збереженням.");
        if (!(await references.GetSnapshotAsync(cancellationToken)).Fabrics.ContainsKey(request.FabricId))
            return Result.NotFound("Тканину не знайдено.");
        var sku = request.SaleSku.Trim();
        if (!string.Equals(sku, article.SaleSku, StringComparison.Ordinal)
            && await store.SaleSkuExistsAsync(sku, command.ArticleId, cancellationToken))
            return Result.Conflict("Такий артикул продажу вже є.");
        var now = TimeProvider.System.GetUtcNow();
        var changed = product.UpdateArticle(articleId, sku, request.Active, now);
        if (changed.IsSuccess)
            changed = product.SetArticleFabric(articleId, FabricReferenceId.Create(request.FabricId),
                request.FabricQuantity, now);
        if (changed.IsFailure) return changed.Error!.ToResult<SewingArticleView>();
        if (!await store.SaveAsync(cancellationToken))
            return Result.Conflict("Склад або артикул уже змінили. Оновіть сторінку перед збереженням.");
        return Result.Success(new SewingArticleView(article.Id.Value, product.Id.Value, article.SaleSku,
            article.IsActive, article.SortOrder, request.FabricId, request.FabricQuantity));
    }
}

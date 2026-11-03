using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Contracts.Reference;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Sewing;

/// <summary>Changes the sale SKU, activity and fabric of one variant (CAT-015).</summary>
public sealed class UpdateSewingArticleHandler(ISewingCompositionStore store, IProductReferenceReader references)
    : ICommandHandler<UpdateSewingArticleCommand, Result<SewingArticleView>>
{
    public async ValueTask<Result<SewingArticleView>> Handle(UpdateSewingArticleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var errors = UpdateSewingArticleCommandValidator.Validate(command);
        if (errors.Count > 0)
            return Result.Invalid(errors);
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        var articleId = ArticleId.Create(command.ArticleId);
        var article = product?.Articles.FirstOrDefault(x => x.Id == articleId);
        if (product is null || article is null)
            return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion)
            return Result.Conflict("Composition changed. Reload before saving.");
        if (!(await references.GetSnapshotAsync(cancellationToken)).Fabrics.ContainsKey(request.FabricId))
            return Result.NotFound("fabricId: fabric not found.");
        var sku = request.SaleSku.Trim();
        if (!string.Equals(sku, article.SaleSku, StringComparison.Ordinal)
            && await store.SaleSkuExistsAsync(sku, command.ArticleId, cancellationToken))
            return Result.Conflict("Sale SKU already exists.");
        var now = TimeProvider.System.GetUtcNow();
        try
        {
            product.UpdateArticle(articleId, sku, request.Active, now);
            product.SetArticleFabric(articleId, FabricReferenceId.Create(request.FabricId), request.FabricQuantity, now);
        }
        catch (DomainException exception)
        {
            return Result.Conflict(exception.Message);
        }
        if (!await store.SaveAsync(cancellationToken))
            return Result.Conflict("Composition or SKU changed. Reload before saving.");
        return Result.Success(new SewingArticleView(article.Id.Value, product.Id.Value, article.SaleSku,
            article.IsActive, article.SortOrder, request.FabricId, request.FabricQuantity));
    }
}

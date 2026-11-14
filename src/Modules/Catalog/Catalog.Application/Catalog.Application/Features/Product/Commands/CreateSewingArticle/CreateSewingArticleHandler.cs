using Catalog.Domain.Products;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Shared.Sewing;

namespace Catalog.Application.Features.Product.Commands.CreateSewingArticle;

/// <summary>Adds a fabric variant: the sale SKU and its fabric in one transaction (CAT-015).</summary>
public sealed class CreateSewingArticleHandler(ISewingCompositionStore store, IProductReferenceReader references)
    : ICommandHandler<CreateSewingArticleCommand, Result<SewingArticleView>>
{
    public async ValueTask<Result<SewingArticleView>> Handle(CreateSewingArticleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion)
            return Result.Conflict("Склад уже змінили. Оновіть сторінку перед збереженням.");
        if (!(await references.GetSnapshotAsync(cancellationToken)).Fabrics.ContainsKey(request.FabricId))
            return Result.NotFound("Тканину не знайдено.");
        if (await store.SaleSkuExistsAsync(request.SaleSku.Trim(), null, cancellationToken))
            return Result.Conflict("Такий артикул продажу вже є.");
        var created = SewingArticle.Create(default, product.Id, request.SaleSku, request.Active, request.SortOrder);
        if (created.IsFailure) return created.Error!.ToResult<SewingArticleView>();
        var article = created.Value;
        var added = await store.AddArticleAsync(product, article, FabricReferenceId.Create(request.FabricId),
            request.FabricQuantity, TimeProvider.System.GetUtcNow(), cancellationToken);
        if (added.IsFailure) return added.Error!.ToResult<SewingArticleView>();
        if (!added.Value)
            return Result.Conflict("Склад або артикул уже змінили. Оновіть сторінку перед збереженням.");
        return Result.Success(new SewingArticleView(article.Id.Value, product.Id.Value, article.SaleSku,
            article.IsActive, article.SortOrder, request.FabricId, request.FabricQuantity));
    }
}

using BuildingBlocks.Domain.Exceptions;
using Catalog.Application.Contracts.Reference;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Sewing;

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
            return Result.Conflict("Composition changed. Reload before saving.");
        if (!(await references.GetSnapshotAsync(cancellationToken)).Fabrics.ContainsKey(request.FabricId))
            return Result.NotFound("fabricId: fabric not found.");
        if (await store.SaleSkuExistsAsync(request.SaleSku.Trim(), null, cancellationToken))
            return Result.Conflict("Sale SKU already exists.");
        var article = SewingArticle.Create(default, product.Id, request.SaleSku, request.Active, request.SortOrder);
        try
        {
            if (!await store.AddArticleAsync(product, article, FabricReferenceId.Create(request.FabricId),
                    request.FabricQuantity, TimeProvider.System.GetUtcNow(), cancellationToken))
                return Result.Conflict("Composition or SKU changed. Reload before saving.");
        }
        catch (DomainException exception)
        {
            return Result.Conflict(exception.Message);
        }
        return Result.Success(new SewingArticleView(article.Id.Value, product.Id.Value, article.SaleSku,
            article.IsActive, article.SortOrder, request.FabricId, request.FabricQuantity));
    }
}

using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Sewing;

public sealed class CreateSewingArticleHandler(ISewingCompositionStore store)
    : ICommandHandler<CreateSewingArticleCommand, Result<SewingArticleView>>
{
    public async ValueTask<Result<SewingArticleView>> Handle(CreateSewingArticleCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var errors = CreateSewingArticleCommandValidator.Validate(command);
        if (errors.Count > 0)
            return Result.Invalid(errors);
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion)
            return Result.Conflict("Composition changed. Reload before saving.");
        if (await store.SaleSkuExistsAsync(request.SaleSku.Trim(), cancellationToken))
            return Result.Conflict("Sale SKU already exists.");
        var article = SewingArticle.Create(default, product.Id, request.SaleSku, request.Active, request.SortOrder);
        try
        {
            product.AddArticle(article, TimeProvider.System.GetUtcNow());
        }
        catch (DomainException exception)
        {
            return Result.Conflict(exception.Message);
        }
        if (!await store.SaveAsync(cancellationToken))
            return Result.Conflict("Composition or SKU changed. Reload before saving.");
        return Result.Success(new SewingArticleView(article.Id.Value, product.Id.Value,
            article.SaleSku, article.IsActive, article.SortOrder));
    }
}

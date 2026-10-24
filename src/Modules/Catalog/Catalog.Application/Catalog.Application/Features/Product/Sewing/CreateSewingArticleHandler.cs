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
        var errors = new List<ValidationError>();
        if (command.ProductId is <= 0 or > 1_000_000_000) errors.Add(new("productId", "Invalid Product ID."));
        if (string.IsNullOrWhiteSpace(request.SaleSku) || request.SaleSku.Trim().Length > 80) errors.Add(new("saleSku", "Provide a sale SKU of up to 80 characters."));
        if (request.SortOrder < 0) errors.Add(new("sortOrder", "Order must be nonnegative."));
        if (request.ExpectedVersion < 0) errors.Add(new("expectedVersion", "Version must be nonnegative."));
        if (errors.Count > 0) return Result.Invalid(errors);
        var product = await store.LoadAsync(command.ProductId, cancellationToken);
        if (product is null) return Result.NotFound();
        if (product.CompositionVersion != request.ExpectedVersion) return Result.Conflict("Composition changed. Reload before saving.");
        if (await store.SaleSkuExistsAsync(request.SaleSku.Trim(), cancellationToken)) return Result.Conflict("Sale SKU already exists.");
        var article = SewingArticle.Create(default, product.Id, request.SaleSku, request.Active, request.SortOrder);
        try { product.AddArticle(article, TimeProvider.System.GetUtcNow()); }
        catch (DomainException exception) { return Result.Conflict(exception.Message); }
        if (!await store.SaveAsync(cancellationToken)) return Result.Conflict("Composition or SKU changed. Reload before saving.");
        return Result.Success(new SewingArticleView(article.Id.Value, product.Id.Value, article.SaleSku, article.IsActive, article.SortOrder));
    }
}

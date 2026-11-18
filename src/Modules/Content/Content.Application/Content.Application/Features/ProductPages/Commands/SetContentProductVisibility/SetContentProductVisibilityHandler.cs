using BuildingBlocks.Domain.Identifiers.Products;
using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using BuildingBlocks.Application;

namespace Content.Application.Features.ProductPages.Commands.SetContentProductVisibility;

public sealed class SetContentProductVisibilityHandler(
    IContentDbContext db, IContentCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<SetContentProductVisibilityCommand, Result<ContentProductVisibilityResponse>>
{
    public async ValueTask<Result<ContentProductVisibilityResponse>> Handle(SetContentProductVisibilityCommand command,
        CancellationToken cancellationToken)
    {
        var product = await catalog.GetAsync(command.ProductId, cancellationToken);
        if (product is null) return Result.NotFound(ContentProductMessages.ProductNotFound);

        var now = clock.GetUtcNow();
        var page = await db.ProductPages.FirstOrDefaultAsync(x => x.ProductId == new ProductId(command.ProductId), cancellationToken);
        if (page is null)
        {
            var slug = await ProductPageSlugs.FreeAsync(db, product, cancellationToken);
            var created = ProductPage.Create(new ProductId(product.Id), slug, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<ContentProductVisibilityResponse>();
            page = created.Value;
            db.ProductPages.Add(page);
        }

        page.SetVisibility(command.ShowOnSite, command.UpdatedBy, now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(ContentProductMessages.Stale);
        }

        return new ContentProductVisibilityResponse(page.ProductId.Value, page.ShowOnSite,
            page.ShowOnSite && product.IsActive, page.Revision, page.Slug);
    }
}

using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Shared;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.ProductPages.Commands.SaveContentProduct;

public sealed class SaveContentProductHandler(IContentDbContext db, IContentCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<SaveContentProductCommand, Result<ContentProductResponse>>
{
    public async ValueTask<Result<ContentProductResponse>> Handle(SaveContentProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await catalog.GetAsync(command.ProductId, cancellationToken);
        if (product is null) return Result.NotFound(ContentProductMessages.ProductNotFound);

        var page = await db.ProductPages.Include(x => x.Texts)
            .FirstOrDefaultAsync(x => x.ProductId == command.ProductId, cancellationToken);
        if ((page?.Revision ?? 0) != command.Revision) return Result.Conflict(ContentProductMessages.Stale);

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? page?.Slug ?? await ProductPageSlugs.FreeAsync(db, product, cancellationToken)
            : command.Slug.Trim();
        if (await ProductPageSlugs.TakenAsync(db, slug, product.Id, cancellationToken))
            return Result.Conflict(ContentProductMessages.SlugTaken);

        if (command.CategoryIds is not null
            && !await ProductCategoryLinks.ReplaceAsync(db, product.Id, command.CategoryIds, cancellationToken))
            return Result.Conflict(ContentProductMessages.CategoryNotFound);

        var now = clock.GetUtcNow();
        var pageOldSlug = page?.Slug;
        if (page is null)
        {
            var created = ProductPage.Create(product.Id, slug, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<ContentProductResponse>();
            page = created.Value;
            db.ProductPages.Add(page);
        }

        var texts = new Dictionary<string, ProductPageTextInput>();
        if (command.Uk is not null) texts[ProductPageRules.Ukrainian] = command.Uk;
        if (command.Ru is not null) texts[ProductPageRules.Russian] = command.Ru;
        var replaced = page.Replace(slug, command.ShowOnSite, texts, command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<ContentProductResponse>();

        // The old address keeps working: a 301 to the new one (CNT-005 R-05).
        if (pageOldSlug is not null && pageOldSlug != slug)
        {
            var redirected = await SiteRedirectWriter.SlugChangedAsync(db, SiteRedirectWriter.Products, pageOldSlug, slug,
                command.UpdatedBy, now, cancellationToken);
            if (redirected.IsFailure) return redirected.Error!.ToResult<ContentProductResponse>();
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(ContentProductMessages.Stale);
        }

        return ContentProductMapping.Detail(product, page, page.Slug,
            await ProductCategoryLinks.IdsAsync(db, product.Id, cancellationToken));
    }
}

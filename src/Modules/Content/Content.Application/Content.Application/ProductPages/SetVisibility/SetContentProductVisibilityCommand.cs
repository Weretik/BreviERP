using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.ProductPages.Shared;
using Content.Domain.ProductPages;

namespace Content.Application.ProductPages.SetVisibility;

/// <summary>
/// «Показувати на сайті» from the list (CNT-002 SC-03). A product without a page gets an empty one with an address
/// built from its name.
/// </summary>
public sealed record SetContentProductVisibilityCommand(int ProductId, bool ShowOnSite, string UpdatedBy)
    : ICommand<Result<ContentProductVisibilityResponse>>;

public sealed class SetContentProductVisibilityCommandValidator : AbstractValidator<SetContentProductVisibilityCommand>
{
    public SetContentProductVisibilityCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}

public sealed class SetContentProductVisibilityCommandHandler(
    IContentDbContext db, IContentCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<SetContentProductVisibilityCommand, Result<ContentProductVisibilityResponse>>
{
    public async ValueTask<Result<ContentProductVisibilityResponse>> Handle(SetContentProductVisibilityCommand command,
        CancellationToken cancellationToken)
    {
        var product = await catalog.GetAsync(command.ProductId, cancellationToken);
        if (product is null) return Result.NotFound(ContentProductMessages.ProductNotFound);

        var now = clock.GetUtcNow();
        var page = await db.ProductPages.FirstOrDefaultAsync(x => x.ProductId == command.ProductId, cancellationToken);
        if (page is null)
        {
            var slug = await ProductPageSlugs.FreeAsync(db, product, cancellationToken);
            page = new ProductPage(product.Id, slug, command.UpdatedBy, now);
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

        return new ContentProductVisibilityResponse(page.ProductId, page.ShowOnSite,
            page.ShowOnSite && product.IsActive, page.Revision, page.Slug);
    }
}

using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;

public sealed class DeleteSiteCategoryHandler(IContentDbContext db)
    : ICommandHandler<DeleteSiteCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteSiteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await db.SiteCategories.Include(x => x.Texts)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (category is null) return Result.NotFound(SiteCategoryMessages.NotFound);
        if (await db.SiteCategories.AnyAsync(x => x.ParentId == category.Id, cancellationToken))
            return Result.Conflict(SiteCategoryMessages.HasSubcategories);

        var links = await db.SiteCategoryProducts.Where(x => x.CategoryId == category.Id).ToListAsync(cancellationToken);
        db.SiteCategoryProducts.RemoveRange(links);
        db.SiteCategories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}

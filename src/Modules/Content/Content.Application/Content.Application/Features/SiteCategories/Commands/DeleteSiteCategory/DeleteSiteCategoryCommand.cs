using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.DeleteSiteCategory;

/// <summary>
/// Removes a category with its texts and its product links (CNT-003 R-04); the products and their pages stay. A
/// category with subcategories is refused.
/// </summary>
public sealed record DeleteSiteCategoryCommand(int Id) : ICommand<Result>;

public sealed class DeleteSiteCategoryValidator : AbstractValidator<DeleteSiteCategoryCommand>
{
    public DeleteSiteCategoryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

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

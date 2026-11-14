using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Redirects.Shared;
using Content.Application.Features.SiteCategories.Shared;
using BuildingBlocks.Application;

namespace Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

public sealed class SaveSiteCategoryHandler(IContentDbContext db, TimeProvider clock)
    : ICommandHandler<SaveSiteCategoryCommand, Result<SiteCategoryResponse>>
{
    public async ValueTask<Result<SiteCategoryResponse>> Handle(SaveSiteCategoryCommand command,
        CancellationToken cancellationToken)
    {
        SiteCategory? category = null;
        if (command.Id is { } id)
        {
            category = await db.SiteCategories.Include(x => x.Texts).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (category is null) return Result.NotFound(SiteCategoryMessages.NotFound);
            if (category.Revision != command.Revision) return Result.Conflict(SiteCategoryMessages.Stale);
        }
        else if (await db.SiteCategories.CountAsync(cancellationToken) >= SiteCategoryRules.MaxCategories)
        {
            return Result.Conflict(SiteCategoryMessages.TooMany);
        }

        if (command.ParentId is { } parentId)
        {
            var parent = await db.SiteCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == parentId, cancellationToken);
            if (parent is null) return Result.Conflict(SiteCategoryMessages.ParentNotFound);
            if (parent.ParentId is not null) return Result.Conflict(SiteCategoryMessages.ParentIsSubcategory);
            if (category is not null && await db.SiteCategories.AnyAsync(x => x.ParentId == category.Id, cancellationToken))
                return Result.Conflict(SiteCategoryMessages.HasSubcategories);
        }

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? category?.Slug ?? await SiteCategoryReads.FreeSlugAsync(db, command.Uk?.Name, cancellationToken)
            : command.Slug.Trim();
        if (await SiteCategoryReads.SlugTakenAsync(db, slug, category?.Id, cancellationToken))
            return Result.Conflict(SiteCategoryMessages.SlugTaken);

        var now = clock.GetUtcNow();
        var categoryOldSlug = category?.Slug;
        var sortOrder = category is not null && category.ParentId == command.ParentId
            ? category.SortOrder
            : await SiteCategoryReads.NextSortOrderAsync(db, command.ParentId, cancellationToken);
        if (category is null)
        {
            var created = SiteCategory.Create(command.ParentId, slug, sortOrder, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<SiteCategoryResponse>();
            category = created.Value;
            db.SiteCategories.Add(category);
        }

        var texts = new Dictionary<string, SiteCategoryTextInput>();
        if (command.Uk is not null) texts[ProductPageRules.Ukrainian] = command.Uk;
        if (command.Ru is not null) texts[ProductPageRules.Russian] = command.Ru;
        var replaced = category.Replace(command.ParentId, sortOrder, slug, command.ShowOnSite, texts, command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<SiteCategoryResponse>();

        // The old address keeps working: a 301 to the new one (CNT-005 R-05).
        if (categoryOldSlug is not null && categoryOldSlug != slug)
        {
            var redirected = await SiteRedirectWriter.SlugChangedAsync(db, SiteRedirectWriter.Categories, categoryOldSlug, slug,
                command.UpdatedBy, now, cancellationToken);
            if (redirected.IsFailure) return redirected.Error!.ToResult<SiteCategoryResponse>();
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(SiteCategoryMessages.Stale);
        }

        return await SiteCategoryReads.DetailAsync(db, category, cancellationToken);
    }
}

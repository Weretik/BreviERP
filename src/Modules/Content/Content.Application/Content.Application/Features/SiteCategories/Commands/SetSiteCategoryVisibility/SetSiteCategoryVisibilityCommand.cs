using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;

/// <summary>«Показувати на сайті» from the tree (CNT-003 SC-03). A subcategory is on the site only with its parent.</summary>
public sealed record SetSiteCategoryVisibilityCommand(int Id, bool ShowOnSite, string UpdatedBy)
    : ICommand<Result<SiteCategoryVisibilityResponse>>;

public sealed class SetSiteCategoryVisibilityValidator : AbstractValidator<SetSiteCategoryVisibilityCommand>
{
    public SetSiteCategoryVisibilityValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}

public sealed class SetSiteCategoryVisibilityHandler(IContentDbContext db, TimeProvider clock)
    : ICommandHandler<SetSiteCategoryVisibilityCommand, Result<SiteCategoryVisibilityResponse>>
{
    public async ValueTask<Result<SiteCategoryVisibilityResponse>> Handle(SetSiteCategoryVisibilityCommand command,
        CancellationToken cancellationToken)
    {
        var category = await db.SiteCategories.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (category is null) return Result.NotFound(SiteCategoryMessages.NotFound);

        category.SetVisibility(command.ShowOnSite, command.UpdatedBy, clock.GetUtcNow());
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(SiteCategoryMessages.Stale);
        }

        var parent = category.ParentId is { } parentId
            ? await db.SiteCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == parentId, cancellationToken)
            : null;
        return new SiteCategoryVisibilityResponse(category.Id, category.ShowOnSite,
            SiteCategoryMapping.OnSite(category, parent), category.Revision);
    }
}

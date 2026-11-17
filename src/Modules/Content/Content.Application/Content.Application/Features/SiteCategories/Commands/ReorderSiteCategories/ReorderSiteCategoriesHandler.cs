using Content.Application.Abstractions;
using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;

public sealed class ReorderSiteCategoriesHandler(IContentDbContext db)
    : ICommandHandler<ReorderSiteCategoriesCommand, Result>
{
    public async ValueTask<Result> Handle(ReorderSiteCategoriesCommand command, CancellationToken cancellationToken)
    {
        var parentId = command.ParentId is { } id ? new SiteCategoryId(id) : (SiteCategoryId?)null;
        var siblings = await db.SiteCategories.Where(x => x.ParentId == parentId).ToListAsync(cancellationToken);
        if (siblings.Count != command.Ids.Count || siblings.Any(x => !command.Ids.Contains(x.Id.Value)))
            return Result.Conflict(SiteCategoryMessages.OrderChanged);

        var byId = siblings.ToDictionary(x => x.Id);
        for (var index = 0; index < command.Ids.Count; index++) byId[new SiteCategoryId(command.Ids[index])].PlaceAt(index + 1);
        await db.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}

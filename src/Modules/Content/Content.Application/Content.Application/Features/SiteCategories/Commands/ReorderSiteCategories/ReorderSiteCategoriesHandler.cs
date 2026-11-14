using Content.Application.Abstractions;
using Content.Domain.SiteCategories;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Commands.ReorderSiteCategories;

public sealed class ReorderSiteCategoriesHandler(IContentDbContext db)
    : ICommandHandler<ReorderSiteCategoriesCommand, Result>
{
    public async ValueTask<Result> Handle(ReorderSiteCategoriesCommand command, CancellationToken cancellationToken)
    {
        var siblings = await db.SiteCategories.Where(x => x.ParentId == command.ParentId).ToListAsync(cancellationToken);
        if (siblings.Count != command.Ids.Count || siblings.Any(x => !command.Ids.Contains(x.Id)))
            return Result.Conflict(SiteCategoryMessages.OrderChanged);

        var byId = siblings.ToDictionary(x => x.Id);
        for (var index = 0; index < command.Ids.Count; index++) byId[command.Ids[index]].PlaceAt(index + 1);
        await db.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}

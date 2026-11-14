using Content.Application.Abstractions;
using Content.Domain.SitePages;
using Content.Application.Features.SitePages.Shared;
using BuildingBlocks.Application;

namespace Content.Application.Features.SitePages.Commands.SaveSitePage;

public sealed class SaveSitePageHandler(IContentDbContext db, TimeProvider clock)
    : ICommandHandler<SaveSitePageCommand, Result<SitePageResponse>>
{
    public async ValueTask<Result<SitePageResponse>> Handle(SaveSitePageCommand command,
        CancellationToken cancellationToken)
    {
        if (SitePageCatalog.Find(command.Key) is not { } entry) return Result.NotFound(SitePageMessages.NotFound);

        var page = await db.SitePages.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);
        if ((page?.Revision ?? 0) != command.Revision) return Result.Conflict(SitePageMessages.Stale);

        var now = clock.GetUtcNow();
        if (page is null)
        {
            page = new SitePage(entry.Key, command.UpdatedBy, now);
            db.SitePages.Add(page);
        }

        var replaced = page.Replace(Input(command.Uk), Input(command.Ru), command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<SitePageResponse>();
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Conflict(SitePageMessages.Stale);
        }

        return SitePageMapping.Response(entry, page);
    }

    private static SitePageTextInput Input(SitePageText text) =>
        new(text.Title, text.Description, text.Text, text.Faq);
}

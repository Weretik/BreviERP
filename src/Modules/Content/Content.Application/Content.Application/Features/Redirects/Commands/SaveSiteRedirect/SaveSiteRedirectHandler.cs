using Content.Application.Abstractions;
using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;
using BuildingBlocks.Application;

namespace Content.Application.Features.Redirects.Commands.SaveSiteRedirect;

public sealed class SaveSiteRedirectHandler(IContentDbContext db, TimeProvider clock)
    : ICommandHandler<SaveSiteRedirectCommand, Result<SiteRedirectResponse>>
{
    public async ValueTask<Result<SiteRedirectResponse>> Handle(SaveSiteRedirectCommand command,
        CancellationToken cancellationToken)
    {
        var from = SiteRedirectRules.Normalize(command.FromPath)!;
        var to = SiteRedirectRules.Normalize(command.ToPath)!;
        SiteRedirect? redirect = null;
        if (command.Id is { } id)
        {
            redirect = await db.SiteRedirects.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (redirect is null) return Result.NotFound(RedirectMessages.NotFound);
        }
        else if (await db.SiteRedirects.CountAsync(cancellationToken) >= SiteRedirectRules.MaxRedirects)
        {
            return Result.Conflict(RedirectMessages.TooMany);
        }

        var ownId = redirect?.Id;
        if (await db.SiteRedirects.AnyAsync(x => x.FromPath == from && x.Id != ownId, cancellationToken))
            return Result.Conflict(RedirectMessages.FromTaken);
        // A redirect from an address another one leads to, or to an address that itself moves, would make a loop.
        if (await db.SiteRedirects.AnyAsync(x => x.Id != ownId && (x.ToPath == from && x.FromPath == to), cancellationToken))
            return Result.Conflict(RedirectMessages.FromIsTarget);

        var now = clock.GetUtcNow();
        if (redirect is null)
        {
            var created = SiteRedirect.Create(from, to, SiteRedirectSource.Manual, command.Note, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<SiteRedirectResponse>();
            redirect = created.Value;
            db.SiteRedirects.Add(redirect);
        }
        else
        {
            var changed = redirect.Change(from, to, command.Note, SiteRedirectSource.Manual, command.UpdatedBy, now);
            if (changed.IsFailure) return changed.Error!.ToResult<SiteRedirectResponse>();
        }

        await db.SaveChangesAsync(cancellationToken);
        return RedirectMapping.Row(redirect);
    }
}

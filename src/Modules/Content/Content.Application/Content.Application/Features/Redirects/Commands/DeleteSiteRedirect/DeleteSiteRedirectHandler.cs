using Content.Application.Abstractions;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.DeleteSiteRedirect;

public sealed class DeleteSiteRedirectHandler(IContentDbContext db)
    : ICommandHandler<DeleteSiteRedirectCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteSiteRedirectCommand command, CancellationToken cancellationToken)
    {
        var redirect = await db.SiteRedirects.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (redirect is null) return Result.NotFound(RedirectMessages.NotFound);
        db.SiteRedirects.Remove(redirect);
        await db.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}

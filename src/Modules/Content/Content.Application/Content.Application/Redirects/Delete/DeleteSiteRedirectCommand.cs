using Content.Application.Abstractions;
using Content.Application.Redirects.Shared;

namespace Content.Application.Redirects.Delete;

/// <summary>Removes a redirect (CNT-005 R-03); the old address then answers 404.</summary>
public sealed record DeleteSiteRedirectCommand(int Id) : ICommand<Result>;

public sealed class DeleteSiteRedirectCommandValidator : AbstractValidator<DeleteSiteRedirectCommand>
{
    public DeleteSiteRedirectCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class DeleteSiteRedirectCommandHandler(IContentDbContext db)
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

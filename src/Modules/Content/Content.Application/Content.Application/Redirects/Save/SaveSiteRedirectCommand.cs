using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.Redirects.Shared;
using Content.Domain.Redirects;

namespace Content.Application.Redirects.Save;

/// <summary>
/// Adds a redirect (<paramref name="Id"/> is null) or changes one (CNT-005 R-03). Both addresses may be typed as full
/// links; they are normalized. A redirect edited by staff becomes <see cref="SiteRedirectSource.Manual"/>.
/// </summary>
public sealed record SaveSiteRedirectCommand(int? Id, string? FromPath, string? ToPath, string? Note, string UpdatedBy)
    : ICommand<Result<SiteRedirectResponse>>;

public sealed class SaveSiteRedirectCommandValidator : AbstractValidator<SaveSiteRedirectCommand>
{
    public SaveSiteRedirectCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id is not null);
        RuleFor(x => x.FromPath).Must(path => SiteRedirectRules.Normalize(path) is not null)
            .WithMessage(RedirectMessages.FromInvalid);
        RuleFor(x => x.ToPath).Must(path => SiteRedirectRules.IsSitePath(SiteRedirectRules.Normalize(path)))
            .WithMessage(RedirectMessages.ToInvalid);
        RuleFor(x => x).Must(x => SiteRedirectRules.Normalize(x.FromPath) != SiteRedirectRules.Normalize(x.ToPath))
            .WithMessage(RedirectMessages.SameAddress).OverridePropertyName("ToPath");
        RuleFor(x => x.Note).MaximumLength(SiteRedirectRules.NoteMaxLength).WithMessage(RedirectMessages.NoteTooLong);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}

public sealed class SaveSiteRedirectCommandHandler(IContentDbContext db, TimeProvider clock)
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

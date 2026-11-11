using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SitePages.Shared;
using Content.Domain.SitePages;

namespace Content.Application.SitePages.Save;

/// <summary>
/// Replaces the search texts of one page with a revision check (CNT-007 SC-02); an empty text gives the page back its
/// own text on the site.
/// </summary>
public sealed record SaveSitePageCommand(
    string Key,
    int Revision,
    SitePageText Uk,
    SitePageText Ru,
    string UpdatedBy) : ICommand<Result<SitePageResponse>>;

public sealed class SaveSitePageCommandValidator : AbstractValidator<SaveSitePageCommand>
{
    public SaveSitePageCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(SitePage.KeyMaxLength);
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Uk).NotNull();
        RuleFor(x => x.Ru).NotNull();
        RuleFor(x => x.Uk.Title).Must(Title).WithMessage(SitePageMessages.TitleTooLong)
            .OverridePropertyName("Uk.Title").When(x => x.Uk is not null);
        RuleFor(x => x.Ru.Title).Must(Title).WithMessage(SitePageMessages.TitleTooLong)
            .OverridePropertyName("Ru.Title").When(x => x.Ru is not null);
        RuleFor(x => x.Uk.Description).Must(Description).WithMessage(SitePageMessages.DescriptionTooLong)
            .OverridePropertyName("Uk.Description").When(x => x.Uk is not null);
        RuleFor(x => x.Ru.Description).Must(Description).WithMessage(SitePageMessages.DescriptionTooLong)
            .OverridePropertyName("Ru.Description").When(x => x.Ru is not null);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }

    private static bool Title(string? value) => (SitePage.Clean(value)?.Length ?? 0) <= SitePage.TitleMaxLength;

    private static bool Description(string? value) =>
        (SitePage.Clean(value)?.Length ?? 0) <= SitePage.DescriptionMaxLength;
}

public sealed class SaveSitePageCommandHandler(IContentDbContext db, TimeProvider clock)
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

        page.Replace(command.Uk.Title, command.Uk.Description, command.Ru.Title, command.Ru.Description,
            command.UpdatedBy, now);
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
}

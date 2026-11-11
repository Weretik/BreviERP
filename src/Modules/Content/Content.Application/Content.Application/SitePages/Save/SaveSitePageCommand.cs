using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SitePages.Shared;
using Content.Domain.SitePages;

namespace Content.Application.SitePages.Save;

/// <summary>
/// Replaces the texts of one page with a revision check (CNT-007 SC-02, CNT-010 SC-01); an empty text gives the page
/// back its own text on the site.
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
        RuleFor(x => x.Uk).NotNull().SetValidator(new SitePageTextValidator("Українська"));
        RuleFor(x => x.Ru).NotNull().SetValidator(new SitePageTextValidator("Російська"));
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}

/// <summary>Shape limits of the texts of one language (CNT-007 R-01, CNT-010 R-01).</summary>
public sealed class SitePageTextValidator : AbstractValidator<SitePageText>
{
    public SitePageTextValidator(string language)
    {
        RuleFor(x => x.Title).Must(value => (SitePage.Clean(value)?.Length ?? 0) <= SitePage.TitleMaxLength)
            .WithMessage(SitePageMessages.TitleTooLong);
        RuleFor(x => x.Description)
            .Must(value => (SitePage.Clean(value)?.Length ?? 0) <= SitePage.DescriptionMaxLength)
            .WithMessage(SitePageMessages.DescriptionTooLong);
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.Count <= SitePage.MaxBlocks)
            .WithMessage($"{language}: {SitePageMessages.TooManyBlocks}");
        RuleFor(x => x.Text).Must(blocks => blocks is null || blocks.All(x => x is not null && Enum.IsDefined(x.Kind)))
            .WithMessage($"{language}: {SitePageMessages.UnknownBlock}");
        RuleFor(x => x.Text).Must(blocks => SitePage.TextLength(blocks) <= SitePage.TextMaxLength)
            .WithMessage($"{language}: {SitePageMessages.TextTooLong}");
        RuleFor(x => x.Faq).Must(items => items is null || items.Count <= SitePage.MaxFaqItems)
            .WithMessage($"{language}: {SitePageMessages.TooManyQuestions}");
        RuleFor(x => x.Faq).Must(items => items is null || items.All(x =>
                x is not null && (SitePage.Clean(x.Question)?.Length ?? 0) <= SitePage.QuestionMaxLength))
            .WithMessage($"{language}: {SitePageMessages.QuestionTooLong}");
        RuleFor(x => x.Faq).Must(items => items is null || items.All(x =>
                x is not null && (SitePage.Clean(x.Answer)?.Length ?? 0) <= SitePage.AnswerMaxLength))
            .WithMessage($"{language}: {SitePageMessages.AnswerTooLong}");
    }
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

        page.Replace(Input(command.Uk), Input(command.Ru), command.UpdatedBy, now);
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

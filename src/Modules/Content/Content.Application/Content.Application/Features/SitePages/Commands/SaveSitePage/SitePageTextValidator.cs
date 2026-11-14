using Content.Application.Abstractions;
using Content.Domain.SitePages;
using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Commands.SaveSitePage;

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

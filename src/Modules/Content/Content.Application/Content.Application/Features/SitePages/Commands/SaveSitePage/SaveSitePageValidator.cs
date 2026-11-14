using Content.Application.Abstractions;
using Content.Domain.SitePages;
using Content.Application.Features.SitePages.Shared;

namespace Content.Application.Features.SitePages.Commands.SaveSitePage;

public sealed class SaveSitePageValidator : AbstractValidator<SaveSitePageCommand>
{
    public SaveSitePageValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(SitePage.KeyMaxLength);
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Uk).NotNull().SetValidator(new SitePageTextValidator("Українська"));
        RuleFor(x => x.Ru).NotNull().SetValidator(new SitePageTextValidator("Російська"));
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}

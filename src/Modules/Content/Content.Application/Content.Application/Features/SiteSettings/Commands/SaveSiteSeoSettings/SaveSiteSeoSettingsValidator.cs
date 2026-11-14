using Content.Application.Abstractions;
using Content.Domain.SiteSettings;
using Content.Application.Features.SiteSettings.Shared;

namespace Content.Application.Features.SiteSettings.Commands.SaveSiteSeoSettings;

public sealed class SaveSiteSeoSettingsValidator : AbstractValidator<SaveSiteSeoSettingsCommand>
{
    public SaveSiteSeoSettingsValidator()
    {
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GoogleAnalyticsId).Must(value => SiteSeoSettings.Clean(value) is not { } id || SiteSeoSettings.IsAnalyticsId(id))
            .WithMessage(SiteSettingsMessages.AnalyticsId);
        RuleFor(x => x.GoogleTagManagerId).Must(value => SiteSeoSettings.Clean(value) is not { } id || SiteSeoSettings.IsTagManagerId(id))
            .WithMessage(SiteSettingsMessages.TagManagerId);
        RuleFor(x => x.GoogleSiteVerification).Must(Verification).WithMessage(SiteSettingsMessages.Verification);
        RuleFor(x => x.BingSiteVerification).Must(Verification).WithMessage(SiteSettingsMessages.Verification);
        RuleFor(x => x.DefaultOgPhotoId).GreaterThan(0).When(x => x.DefaultOgPhotoId is not null);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }

    private static bool Verification(string? value) =>
        SiteSeoSettings.Clean(value) is not { } code || SiteSeoSettings.IsVerification(code);
}

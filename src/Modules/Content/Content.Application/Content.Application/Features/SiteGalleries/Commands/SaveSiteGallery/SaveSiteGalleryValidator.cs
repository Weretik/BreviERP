using Content.Application.Abstractions;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SiteGalleries.Shared.Admin;

namespace Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;

public sealed class SaveSiteGalleryValidator : AbstractValidator<SaveSiteGalleryCommand>
{
    public SaveSiteGalleryValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(SiteGallery.KeyMaxLength);
        RuleFor(x => x.Revision).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Items).NotNull();
        RuleFor(x => x.Items).Must(items => items.Count <= SiteGallery.MaxWorks)
            .WithMessage(SiteGalleryMessages.TooManyWorks).When(x => x.Items is not null && x.Key != SiteGallery.Certificates);
        RuleFor(x => x.Items).Must(items => items.Count <= SiteGallery.MaxCertificates)
            .WithMessage(SiteGalleryMessages.TooManyCertificates).When(x => x.Items is not null && x.Key == SiteGallery.Certificates);
        RuleFor(x => x.Items).Must(items => items.All(x => x is not null && (x.PhotoId is > 0 || x.LegacyPath is not null)))
            .WithMessage(SiteGalleryMessages.NoPicture).When(x => x.Items is not null);
        RuleFor(x => x.Items).Must(items => items.Where(x => x is not null).All(x => x.LegacyPath is null || SiteGallery.IsLegacyPath(x.LegacyPath)))
            .WithMessage(SiteGalleryMessages.UnknownFile).When(x => x.Items is not null);
        RuleFor(x => x.Items).Must(Unique).WithMessage(SiteGalleryMessages.Twice).When(x => x.Items is not null);
        RuleFor(x => x.Items).Must(items => items.Where(x => x is not null).All(x =>
                Fits(x.Title, SiteGallery.TitleMaxLength)))
            .WithMessage(SiteGalleryMessages.TitleTooLong).When(x => x.Items is not null);
        RuleFor(x => x.Items).Must(items => items.Where(x => x is not null).All(x => Fits(x.Note, SiteGallery.NoteMaxLength)))
            .WithMessage(SiteGalleryMessages.NoteTooLong).When(x => x.Items is not null);
        RuleFor(x => x.Items).Must(items => items.Where(x => x is not null).All(x =>
                (x.Standards ?? []).Count <= SiteGallery.MaxStandards
                && (x.Standards ?? []).All(s => (SiteCompanyInfo.Clean(s)?.Length ?? 0) <= SiteGallery.StandardMaxLength)))
            .WithMessage(SiteGalleryMessages.Standards).When(x => x.Items is not null);
        RuleFor(x => x.Items).Must(items => items.Where(x => x is not null).All(x => SiteCompanyInfo.Clean(x.Title?.Uk) is not null))
            .WithMessage(SiteGalleryMessages.NoTitle).When(x => x.Items is not null && x.Key == SiteGallery.Certificates);
    }

    private static bool Unique(IReadOnlyList<SiteGalleryItemInput> items)
    {
        var present = items.Where(x => x is not null).ToArray();
        var photos = present.Where(x => x.PhotoId is not null).Select(x => x.PhotoId!.Value).ToArray();
        var files = present.Where(x => x.PhotoId is null && x.LegacyPath is not null).Select(x => x.LegacyPath!).ToArray();
        return photos.Distinct().Count() == photos.Length && files.Distinct().Count() == files.Length;
    }

    private static bool Fits(GalleryText? text, int max) =>
        (SiteCompanyInfo.Clean(text?.Uk)?.Length ?? 0) <= max && (SiteCompanyInfo.Clean(text?.Ru)?.Length ?? 0) <= max;
}

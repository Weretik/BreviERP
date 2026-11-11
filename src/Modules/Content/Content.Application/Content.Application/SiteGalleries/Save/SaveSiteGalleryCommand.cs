using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteGalleries.Shared;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;

namespace Content.Application.SiteGalleries.Save;

/// <summary>
/// Replaces the pictures of «Наші роботи» or «Сертифікати» as a whole with a revision check (CNT-011 SC-02). A file the
/// site still keeps can stay in the list but cannot be added; new pictures come from the «Фото» library.
/// </summary>
public sealed record SaveSiteGalleryCommand(
    string Key,
    int Revision,
    IReadOnlyList<SiteGalleryItemInput> Items,
    string UpdatedBy) : ICommand<Result<SiteGalleryResponse>>;

public sealed class SaveSiteGalleryCommandValidator : AbstractValidator<SaveSiteGalleryCommand>
{
    public SaveSiteGalleryCommandValidator()
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

public sealed class SaveSiteGalleryCommandHandler(IContentDbContext db, IContentMediaReader media, TimeProvider clock)
    : ICommandHandler<SaveSiteGalleryCommand, Result<SiteGalleryResponse>>
{
    public async ValueTask<Result<SiteGalleryResponse>> Handle(SaveSiteGalleryCommand command,
        CancellationToken cancellationToken)
    {
        if (!SiteGallery.IsKey(command.Key)) return Result.NotFound(SiteGalleryMessages.NotFound);
        var gallery = await db.SiteGalleries.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);
        if ((gallery?.Revision ?? 0) != command.Revision) return Result.Conflict(SiteGalleryMessages.Stale);

        // A site file keeps the sizes the gallery already knows; one it does not know cannot be added.
        var files = (gallery?.Items ?? []).Where(x => x.Legacy is not null).ToDictionary(x => x.Legacy!.Path, x => x.Legacy!);
        var items = new List<SiteGalleryItem>();
        foreach (var input in command.Items)
        {
            SiteGalleryLegacyPhoto? legacy = null;
            if (input.PhotoId is null && !files.TryGetValue(input.LegacyPath!, out legacy))
                return Result.Invalid(new ValidationError(SiteGalleryMessages.UnknownFile));
            items.Add(new SiteGalleryItem(input.PhotoId, legacy, Text(input.Title), Text(input.Note), input.Standards));
        }

        var known = await SiteGalleryReads.PhotosAsync(items, media, cancellationToken);
        var added = items.Where(x => x.PhotoId is not null).Select(x => x.PhotoId!.Value)
            .Except((gallery?.Items ?? []).Where(x => x.PhotoId is not null).Select(x => x.PhotoId!.Value));
        if (added.Any(id => !known.ContainsKey(id)))
            return Result.Invalid(new ValidationError(SiteGalleryMessages.MissingPhoto));

        var now = clock.GetUtcNow();
        if (gallery is null)
        {
            gallery = new SiteGallery(command.Key, command.UpdatedBy, now);
            db.SiteGalleries.Add(gallery);
        }

        gallery.Replace(items, command.UpdatedBy, now);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result.Conflict(SiteGalleryMessages.Stale);
        }

        return await SiteGalleryReads.ResponseAsync(command.Key, gallery, media, cancellationToken);
    }

    private static SiteText? Text(GalleryText? text) =>
        text is null ? null : new SiteText(text.Uk ?? string.Empty, text.Ru ?? string.Empty);
}

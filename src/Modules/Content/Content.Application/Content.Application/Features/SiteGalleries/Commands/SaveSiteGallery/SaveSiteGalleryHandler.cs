using Content.Application.Abstractions;
using Content.Domain.SiteCompany;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Application.Features.SiteGalleries.Shared.Admin;
using BuildingBlocks.Application;

namespace Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;

public sealed class SaveSiteGalleryHandler(IContentDbContext db, IContentMediaReader media, TimeProvider clock)
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
            var created = SiteGallery.Create(command.Key, command.UpdatedBy, now);
            if (created.IsFailure) return created.Error!.ToResult<SiteGalleryResponse>();
            gallery = created.Value;
            db.SiteGalleries.Add(gallery);
        }

        var replaced = gallery.Replace(items, command.UpdatedBy, now);
        if (replaced.IsFailure) return replaced.Error!.ToResult<SiteGalleryResponse>();
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

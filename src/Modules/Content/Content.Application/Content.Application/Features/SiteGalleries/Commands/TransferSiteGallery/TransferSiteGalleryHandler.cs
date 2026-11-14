using Content.Application.Abstractions;
using Content.Domain.SiteGalleries;
using Content.Application.Features.SiteGalleries.Shared;

namespace Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;

public sealed class TransferSiteGalleryHandler(
    IContentDbContext db, IContentPhotoImporter photos, IContentMediaReader media, TimeProvider clock)
    : ICommandHandler<TransferSiteGalleryCommand, Result<SiteGalleryTransferResponse>>
{
    public const string Failed = "Не вдалося перенести";

    public async ValueTask<Result<SiteGalleryTransferResponse>> Handle(TransferSiteGalleryCommand command,
        CancellationToken cancellationToken)
    {
        if (!SiteGallery.IsKey(command.Key)) return Result.NotFound(SiteGalleryMessages.NotFound);
        var gallery = await db.SiteGalleries.FirstOrDefaultAsync(x => x.Key == command.Key, cancellationToken);
        if (gallery is null) return Result.NotFound(SiteGalleryMessages.NotFound);

        var waiting = gallery.Items.Where(x => x.Legacy is not null).Select(x => x.Legacy!.Path).ToArray();
        var batch = waiting.Take(SiteGalleryFiles.TransferBatch).ToArray();
        var known = await photos.FindImportedAsync(batch.Select(SiteGalleryFiles.SourceKey).ToArray(), cancellationToken);
        var problems = new List<string>();
        var moved = 0;
        var now = clock.GetUtcNow();
        foreach (var path in batch)
        {
            var key = SiteGalleryFiles.SourceKey(path);
            int? photoId = known.TryGetValue(key, out var id) ? id : null;
            if (photoId is null)
            {
                var download = await photos.DownloadAsync(SiteGalleryFiles.Url(path), key,
                    SiteGalleryFiles.Folder(command.Key), cancellationToken);
                photoId = download.PhotoId;
                if (photoId is null) problems.Add($"{Failed} {path}: {download.Error ?? "невідома причина"}.");
            }

            if (photoId is null) continue;
            gallery.UseLibraryPhoto(path, photoId.Value, command.UpdatedBy, now);
            moved++;
        }

        if (moved > 0) await db.SaveChangesAsync(cancellationToken);
        // A failed picture stays a site file; the Admin stops repeating when a request moves nothing.
        return new SiteGalleryTransferResponse(moved, waiting.Length - moved, problems,
            await SiteGalleryReads.ResponseAsync(command.Key, gallery, media, cancellationToken));
    }
}

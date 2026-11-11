using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteGalleries.Shared;
using Content.Domain.SiteGalleries;

namespace Content.Application.SiteGalleries.Transfer;

/// <summary>
/// «Перенести у бібліотеку фото» (CNT-011 SC-03): downloads the next pictures the site still keeps as its own files into
/// the «Фото» library and puts the library photos in their place. One request moves up to
/// <see cref="SiteGalleryFiles.TransferBatch"/> pictures; the Admin repeats it while some are left. A picture already in
/// the library is not downloaded again.
/// </summary>
public sealed record TransferSiteGalleryCommand(string Key, string UpdatedBy) : ICommand<Result<SiteGalleryTransferResponse>>;

public sealed class TransferSiteGalleryCommandValidator : AbstractValidator<TransferSiteGalleryCommand>
{
    public TransferSiteGalleryCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(SiteGallery.KeyMaxLength);
        RuleFor(x => x.UpdatedBy).NotEmpty();
    }
}

public sealed class TransferSiteGalleryCommandHandler(
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

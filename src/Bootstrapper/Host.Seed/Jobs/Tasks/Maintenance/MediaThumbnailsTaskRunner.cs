using Microsoft.Extensions.Logging;
using Media.Application.Features.Files.Commands.BackfillMediaPreviews;

namespace Host.Seed.Jobs.Tasks.Maintenance;

/// <summary>
/// Photo previews (MED-001 stage 3): makes dimensions and WebP previews for library photos that have none yet,
/// such as photos uploaded before previews existed. At most <see cref="BatchSize"/> photos per run; on a normal
/// day it finds nothing.
/// </summary>
internal sealed class MediaThumbnailsTaskRunner : IScheduledTaskRunner
{
    public const int BatchSize = 500;

    public string Key => "media-thumbnails";

    public async Task<string> RunAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken)
    {
        var report = await services.SendAsync(new BackfillMediaPreviewsCommand(BatchSize), cancellationToken);
        logger.LogInformation(
            "Processed {Processed} photos ({WithoutPreview} small or unreadable, no preview), failed {Failed}, left for next runs {Remaining}",
            report.Processed, report.WithoutPreview, report.Failed, report.Remaining);
        return $"Оброблено фото {report.Processed}, з помилкою {report.Failed}, залишилось {report.Remaining}.";
    }
}

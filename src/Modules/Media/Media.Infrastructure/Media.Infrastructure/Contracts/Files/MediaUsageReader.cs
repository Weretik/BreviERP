using Media.Application.Abstractions;
using Media.Contracts.Files;

namespace Media.Infrastructure.Contracts.Files;

/// <summary>Asks every module that keeps library photos which of them it uses (ADR-0019).</summary>
internal sealed class MediaUsageReader(IEnumerable<IMediaFileUsageSource> sources) : IMediaUsageReader
{
    public async Task<IReadOnlyList<MediaUsage>> FindAsync(IReadOnlyCollection<int> fileIds,
        CancellationToken cancellationToken)
    {
        var usages = new List<MediaUsage>();
        foreach (var source in sources)
            usages.AddRange((await source.UsagesAsync(fileIds, cancellationToken))
                .Select(x => new MediaUsage(x.MediaFileId, x.Area, checked((int)x.OwnerId), x.Title)));
        return usages;
    }
}

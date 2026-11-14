using Mediator;
using Media.Application.Files.GetFilesByIds;
using Media.Contracts.Files;

namespace Media.Infrastructure.Contracts.Files;

internal sealed class MediaFileReader(ISender sender) : IMediaFileReader
{
    public async Task<IReadOnlyList<MediaFileInfo>> GetByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return [];
        var result = await sender.Send(new GetMediaFilesByIdsQuery(ids), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Media library photos could not be read.");
        return result.Value
            .Select(x => new MediaFileInfo(x.Id, x.FileName, x.PublicUrl, x.ThumbnailUrl, x.Width, x.Height))
            .ToArray();
    }
}

using Content.Application.Abstractions;

using Media.Contracts.Files;

namespace Content.Infrastructure.Adapters.Media;

/// <summary>Gives Content the photos of the «Фото» library by id.</summary>
internal sealed class ContentMediaReader(IMediaFileReader files) : IContentMediaReader
{
    public async Task<IReadOnlyList<ContentMediaPhoto>> GetAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken) =>
        (await files.GetByIdsAsync(ids, cancellationToken))
        .Select(x => new ContentMediaPhoto(x.Id, x.PublicUrl, x.ThumbnailUrl, x.Width, x.Height)).ToArray();
}

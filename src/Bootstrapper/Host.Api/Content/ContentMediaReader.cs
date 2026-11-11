using Content.Application.Abstractions;
using Mediator;
using Media.Application.Files.GetFilesByIds;

namespace Host.Api.Content;

/// <summary>Gives Content the photos of the «Фото» library by id.</summary>
internal sealed class ContentMediaReader(ISender sender) : IContentMediaReader
{
    public async Task<IReadOnlyList<ContentMediaPhoto>> GetAsync(IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return [];
        var result = await sender.Send(new GetMediaFilesByIdsQuery(ids), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Media library photos could not be read.");
        return result.Value.Select(x => new ContentMediaPhoto(x.Id, x.PublicUrl, x.ThumbnailUrl, x.Width, x.Height)).ToArray();
    }
}

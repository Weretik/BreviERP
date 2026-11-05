using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mockup;
using Mediator;
using Media.Application.Files.GetFilesByIds;

namespace Host.Api.Crm;

/// <summary>«Макет» of a saved embroidery calculation: its photo from the Media library.</summary>
internal sealed class EmbroideryMediaReader(ISender sender) : IEmbroideryMediaReader
{
    public async Task<IReadOnlyDictionary<int, EmbroideryMockupResponse>> ReadAsync(
        IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken)
    {
        if (mediaFileIds.Count == 0) return new Dictionary<int, EmbroideryMockupResponse>();
        var result = await sender.Send(new GetMediaFilesByIdsQuery(mediaFileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Media library photos could not be read.");
        return result.Value.ToDictionary(x => x.Id,
            x => new EmbroideryMockupResponse(x.Id, x.FileName, x.PublicUrl, x.ThumbnailUrl));
    }
}

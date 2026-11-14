
using Media.Contracts.Files;
using Crm.Application.Features.Embroidery.Shared;
using Crm.Application.Abstractions.Embroidery;

namespace Crm.Infrastructure.Adapters.Media;

/// <summary>«Макет» of a saved embroidery calculation: its photo from the Media library.</summary>
internal sealed class EmbroideryMediaReader(IMediaFileReader files) : IEmbroideryMediaReader
{
    public async Task<IReadOnlyDictionary<int, EmbroideryMockupResponse>> ReadAsync(
        IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken) =>
        (await files.GetByIdsAsync(mediaFileIds, cancellationToken)).ToDictionary(x => x.Id,
            x => new EmbroideryMockupResponse(x.Id, x.FileName, x.PublicUrl, x.ThumbnailUrl));
}

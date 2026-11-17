using Crm.Application.Features.Embroidery.Queries.GetEmbroideryMockupUsages;
using Mediator;
using Media.Contracts.Files;

namespace Crm.Infrastructure.Adapters.Media;

/// <summary>Tells the library which «Ціна вишивки» calculations use a photo as their «Макет» (ADR-0019).</summary>
internal sealed class EmbroideryMediaUsage(ISender sender) : IMediaFileUsageSource
{
    public async Task<IReadOnlyList<MediaFileUsage>> UsagesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetEmbroideryMockupUsagesQuery(mediaFileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Embroidery mockup usages could not be read.");
        return result.Value.Select(x => new MediaFileUsage(x.MediaFileId, "Вишивка", x.CalculationId, x.Title))
            .ToArray();
    }
}

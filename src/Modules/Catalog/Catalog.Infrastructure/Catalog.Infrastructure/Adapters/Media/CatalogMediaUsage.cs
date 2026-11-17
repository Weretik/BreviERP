using Catalog.Application.Features.Product.Queries.GetProductMediaUsages;
using Mediator;
using Media.Contracts.Files;

namespace Catalog.Infrastructure.Adapters.Media;

/// <summary>Tells the library which product cards use a photo (ADR-0019).</summary>
internal sealed class CatalogMediaUsage(ISender sender) : IMediaFileUsageSource
{
    public async Task<IReadOnlyList<MediaFileUsage>> UsagesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductMediaUsagesQuery(mediaFileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Catalog photo usages could not be read.");
        return result.Value.Select(x => new MediaFileUsage(x.MediaFileId, "Товари", x.ProductId, x.ProductName))
            .ToArray();
    }
}

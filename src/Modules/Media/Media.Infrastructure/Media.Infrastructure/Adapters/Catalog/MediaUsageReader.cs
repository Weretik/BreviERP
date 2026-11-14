using Catalog.Contracts.Products;
using Crm.Contracts.Embroidery;
using Media.Application.Abstractions;

namespace Media.Infrastructure.Adapters.Catalog;

/// <summary>Tells the library which Catalog products and which embroidery «Макет» use a photo.</summary>
internal sealed class MediaUsageReader(ICatalogUsage catalog, IEmbroideryCalculationReader embroidery)
    : IMediaUsageReader
{
    public async Task<IReadOnlyList<MediaUsage>> FindAsync(IReadOnlyCollection<int> fileIds,
        CancellationToken cancellationToken)
    {
        var products = await catalog.MediaUsagesAsync(fileIds, cancellationToken);
        var mockups = await embroidery.MockupUsagesAsync(fileIds, cancellationToken);
        return products.Select(x => new MediaUsage(x.MediaFileId, "Товари", x.ProductId, x.ProductName))
            .Concat(mockups.Select(x => new MediaUsage(x.MediaFileId, "Вишивка", checked((int)x.CalculationId),
                x.Title)))
            .ToList();
    }
}

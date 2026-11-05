using Catalog.Application.Features.Product.GetMediaUsages;
using Crm.Application.Embroidery.Mockup;
using Mediator;
using Media.Application.Abstractions;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>Tells the Media library which Catalog products and which embroidery «Макет» use a photo.</summary>
internal sealed class MediaProductUsageReader(ISender sender) : IMediaUsageReader
{
    public async Task<IReadOnlyList<MediaUsage>> FindAsync(IReadOnlyCollection<int> fileIds, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductMediaUsagesQuery(fileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Catalog photo usages could not be read.");
        var embroidery = await sender.Send(new GetEmbroideryMockupUsagesQuery(fileIds), cancellationToken);
        if (!embroidery.IsSuccess) throw new InvalidOperationException("Embroidery mockup usages could not be read.");
        return result.Value.Select(x => new MediaUsage(x.MediaFileId, "Товари", x.ProductId, x.ProductName))
            .Concat(embroidery.Value.Select(x => new MediaUsage(x.MediaFileId, "Вишивка", checked((int)x.CalculationId),
                x.Title)))
            .ToList();
    }
}

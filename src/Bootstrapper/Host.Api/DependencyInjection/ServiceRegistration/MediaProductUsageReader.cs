using Catalog.Application.Features.Product.GetMediaUsages;
using Mediator;
using Media.Application.Abstractions;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>Tells the Media library which Catalog products use a photo.</summary>
internal sealed class MediaProductUsageReader(ISender sender) : IMediaUsageReader
{
    public async Task<IReadOnlyList<MediaUsage>> FindAsync(IReadOnlyCollection<int> fileIds, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductMediaUsagesQuery(fileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Catalog photo usages could not be read.");
        return result.Value.Select(x => new MediaUsage(x.MediaFileId, "Товари", x.ProductId, x.ProductName)).ToList();
    }
}

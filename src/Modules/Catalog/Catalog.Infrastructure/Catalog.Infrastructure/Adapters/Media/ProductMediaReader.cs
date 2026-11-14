
using Media.Contracts.Files;
using Catalog.Application.Abstractions.Media;

namespace Catalog.Infrastructure.Adapters.Media;

/// <summary>Gives Catalog product photos their URLs from the Media library.</summary>
internal sealed class ProductMediaReader(IMediaFileReader files) : IProductMediaReader
{
    public async Task<IReadOnlyDictionary<int, string>> GetUrlsAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken) =>
        (await files.GetByIdsAsync(mediaFileIds, cancellationToken)).ToDictionary(x => x.Id, x => x.PublicUrl);

    public async Task<IReadOnlyDictionary<int, ProductMediaFile>> GetFilesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken) =>
        (await files.GetByIdsAsync(mediaFileIds, cancellationToken)).ToDictionary(x => x.Id,
            x => new ProductMediaFile(x.Id, x.FileName, x.PublicUrl, x.ThumbnailUrl));
}

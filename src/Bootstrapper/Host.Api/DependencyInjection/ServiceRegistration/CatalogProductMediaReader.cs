using Catalog.Application.Contracts.Media;
using Mediator;
using Media.Application.Files.GetFilesByIds;

namespace Host.Api.DependencyInjection.ServiceRegistration;

/// <summary>Gives Catalog product photos their URLs from the Media library.</summary>
internal sealed class CatalogProductMediaReader(ISender sender) : IProductMediaReader
{
    public async Task<IReadOnlyDictionary<int, string>> GetUrlsAsync(IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken)
    {
        if (mediaFileIds.Count == 0) return new Dictionary<int, string>();
        var result = await sender.Send(new GetMediaFilesByIdsQuery(mediaFileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Media library photos could not be read.");
        return result.Value.ToDictionary(x => x.Id, x => x.PublicUrl);
    }

    public async Task<IReadOnlyDictionary<int, ProductMediaFile>> GetFilesAsync(IReadOnlyCollection<int> mediaFileIds, CancellationToken cancellationToken)
    {
        if (mediaFileIds.Count == 0) return new Dictionary<int, ProductMediaFile>();
        var result = await sender.Send(new GetMediaFilesByIdsQuery(mediaFileIds), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Media library photos could not be read.");
        return result.Value.ToDictionary(x => x.Id, x => new ProductMediaFile(x.Id, x.FileName, x.PublicUrl, x.ThumbnailUrl));
    }
}

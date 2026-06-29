using Catalog.Domain.Media.Entities;

namespace Catalog.Application.Features.Media.Shared.Specifications;

public sealed class MediaFileByStorageKeySpec : Specification<MediaFile>
{
    public MediaFileByStorageKeySpec(string storageKey)
    {
        Query.Where(x => x.StorageKey == storageKey);
    }
}

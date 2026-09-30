using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;

namespace Catalog.Application.Features.Media.Shared.Specifications;

public sealed class MediaFileByIdSpec : Specification<MediaFile>
{
    public MediaFileByIdSpec(int id)
    {
        var mediaFileId = MediaFileId.Create(id);
        Query.Where(x => x.Id == mediaFileId);
    }
}

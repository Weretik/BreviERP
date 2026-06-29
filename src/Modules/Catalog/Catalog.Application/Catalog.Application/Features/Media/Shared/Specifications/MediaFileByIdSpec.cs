using Catalog.Domain.Media.Entities;

namespace Catalog.Application.Features.Media.Shared.Specifications;

public sealed class MediaFileByIdSpec : Specification<MediaFile>
{
    public MediaFileByIdSpec(int id)
    {
        Query.Where(x => x.Id.Value == id);
    }
}

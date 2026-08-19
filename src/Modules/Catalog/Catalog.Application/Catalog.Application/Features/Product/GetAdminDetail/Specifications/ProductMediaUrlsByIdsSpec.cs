using Catalog.Application.Features.Product.Shared.DTOs;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;

namespace Catalog.Application.Features.Product.GetAdminDetail.Specifications;

public sealed class ProductMediaUrlsByIdsSpec : Specification<MediaFile, ProductMediaUrl>
{
    public ProductMediaUrlsByIdsSpec(IReadOnlyCollection<int> mediaFileIds)
    {
        var ids = mediaFileIds.Select(MediaFileId.Create).ToArray();
        Query.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.PublicUrl != null)
            .Select(x => new ProductMediaUrl(x.Id.Value, x.PublicUrl!));
    }
}

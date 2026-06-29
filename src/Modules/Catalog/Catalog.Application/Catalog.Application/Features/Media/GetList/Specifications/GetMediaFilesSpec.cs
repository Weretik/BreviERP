using Catalog.Application.Features.Media.GetList.DTOs;
using Catalog.Domain.Media.Entities;

namespace Catalog.Application.Features.Media.GetList.Specifications;

public sealed class GetMediaFilesSpec : Specification<MediaFile, MediaFileListItemDto>
{
    public GetMediaFilesSpec()
    {
        Query.OrderByDescending(x => x.Id.Value);

        Query.Select(x => new MediaFileListItemDto(
            x.Id.Value,
            x.FileName,
            x.PublicUrl ?? string.Empty,
            x.ContentType,
            x.StorageKey,
            x.Status.ToString()));
    }
}

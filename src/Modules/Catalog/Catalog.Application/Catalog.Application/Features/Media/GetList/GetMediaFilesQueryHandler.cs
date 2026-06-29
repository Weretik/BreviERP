using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Media.GetList.DTOs;
using Catalog.Application.Features.Media.GetList.Specifications;
using Catalog.Domain.Media.Entities;

namespace Catalog.Application.Features.Media.GetList;

public sealed class GetMediaFilesQueryHandler(ICatalogReadRepository<MediaFile> repository)
    : IQueryHandler<GetMediaFilesQuery, Result<List<MediaFileListItemDto>>>
{
    public async ValueTask<Result<List<MediaFileListItemDto>>> Handle(
        GetMediaFilesQuery query,
        CancellationToken cancellationToken)
    {
        var items = await repository.ListAsync(new GetMediaFilesSpec(), cancellationToken);
        return Result.Success(items);
    }
}

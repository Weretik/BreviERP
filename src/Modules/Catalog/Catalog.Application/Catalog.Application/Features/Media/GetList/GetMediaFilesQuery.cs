using Catalog.Application.Features.Media.GetList.DTOs;

namespace Catalog.Application.Features.Media.GetList;

public sealed record GetMediaFilesQuery : IQuery<Result<List<MediaFileListItemDto>>>;

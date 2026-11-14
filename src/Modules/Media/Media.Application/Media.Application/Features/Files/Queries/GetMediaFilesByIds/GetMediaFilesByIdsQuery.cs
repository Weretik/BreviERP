using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Queries.GetMediaFilesByIds;

/// <summary>Photos by id for other modules (Catalog product photos). Missing ids are left out.</summary>
public sealed record GetMediaFilesByIdsQuery(IReadOnlyCollection<int> Ids) : IQuery<Result<IReadOnlyList<MediaFileResponse>>>;

using Media.Application.Contracts;

namespace Media.Application.Files.GetFilesByIds;

/// <summary>Photos by id for other modules (Catalog product photos). Missing ids are left out.</summary>
public sealed record GetMediaFilesByIdsQuery(IReadOnlyCollection<int> Ids) : IQuery<Result<IReadOnlyList<MediaFileResponse>>>;

using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Queries.GetMediaFile;

/// <summary>One photo and where it is used.</summary>
public sealed record GetMediaFileQuery(int Id) : IQuery<Result<MediaFileDetailResponse>>;

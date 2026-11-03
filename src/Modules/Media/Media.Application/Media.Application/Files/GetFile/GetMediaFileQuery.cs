using Media.Application.Contracts;

namespace Media.Application.Files.GetFile;

/// <summary>One photo and where it is used.</summary>
public sealed record GetMediaFileQuery(int Id) : IQuery<Result<MediaFileDetailResponse>>;

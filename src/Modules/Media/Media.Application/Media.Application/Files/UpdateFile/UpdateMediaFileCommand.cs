using Media.Application.Contracts;

namespace Media.Application.Files.UpdateFile;

/// <summary>Renames the photo, changes its default alt text and/or folder. The CDN file and URL stay the same.</summary>
public sealed record UpdateMediaFileCommand(int Id, MediaFileInput Input) : ICommand<Result<MediaFileResponse>>;

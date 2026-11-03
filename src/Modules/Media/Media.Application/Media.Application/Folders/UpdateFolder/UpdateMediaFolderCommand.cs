using Media.Application.Contracts;

namespace Media.Application.Folders.UpdateFolder;

/// <summary>Renames the folder and/or moves it under another parent (null = top level).</summary>
public sealed record UpdateMediaFolderCommand(int Id, MediaFolderInput Input) : ICommand<Result<MediaFolderResponse>>;

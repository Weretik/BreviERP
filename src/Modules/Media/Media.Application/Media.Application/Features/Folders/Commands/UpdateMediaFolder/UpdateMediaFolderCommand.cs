using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Commands.UpdateMediaFolder;

/// <summary>Renames the folder and/or moves it under another parent (null = top level).</summary>
public sealed record UpdateMediaFolderCommand(int Id, MediaFolderInput Input) : ICommand<Result<MediaFolderResponse>>;

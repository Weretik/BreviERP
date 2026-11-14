namespace Media.Application.Features.Folders.Commands.DeleteMediaFolder;

/// <summary>Deletes an empty folder: no photos and no subfolders.</summary>
public sealed record DeleteMediaFolderCommand(int Id) : ICommand<Result>;

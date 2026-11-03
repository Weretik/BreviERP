namespace Media.Application.Folders.DeleteFolder;

/// <summary>Deletes an empty folder: no photos and no subfolders.</summary>
public sealed record DeleteMediaFolderCommand(int Id) : ICommand<Result>;

namespace Media.Application.Files.DeleteFile;

/// <summary>Deletes a photo from the CDN and the library, unless something still uses it.</summary>
public sealed record DeleteMediaFileCommand(int Id) : ICommand<Result>;

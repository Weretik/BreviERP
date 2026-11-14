namespace Media.Application.Features.Files.Commands.DeleteMediaFile;

/// <summary>Deletes a photo from the CDN and the library, unless something still uses it.</summary>
public sealed record DeleteMediaFileCommand(int Id) : ICommand<Result>;

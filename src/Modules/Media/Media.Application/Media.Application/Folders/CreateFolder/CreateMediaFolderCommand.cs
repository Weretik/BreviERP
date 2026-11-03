using Media.Application.Contracts;

namespace Media.Application.Folders.CreateFolder;

public sealed record CreateMediaFolderCommand(MediaFolderInput Input) : ICommand<Result<MediaFolderResponse>>;

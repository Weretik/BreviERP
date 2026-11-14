using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Commands.CreateMediaFolder;

public sealed record CreateMediaFolderCommand(MediaFolderInput Input) : ICommand<Result<MediaFolderResponse>>;

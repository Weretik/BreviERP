using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Commands.MoveMediaFiles;

/// <summary>Moves several photos into one folder (null = library root).</summary>
public sealed record MoveMediaFilesCommand(MoveMediaFilesInput Input) : ICommand<Result>
{
    public const int MaxFiles = 200;
}

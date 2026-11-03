using Media.Application.Contracts;

namespace Media.Application.Files.MoveFiles;

/// <summary>Moves several photos into one folder (null = library root).</summary>
public sealed record MoveMediaFilesCommand(MoveMediaFilesInput Input) : ICommand<Result>
{
    public const int MaxFiles = 200;
}

namespace Media.Application.Features.Folders.Commands.EnsureMediaFolderPath;

/// <summary>
/// Finds the folder at the path of names from the library root (for example «Старий сайт / costum_leto / agier»),
/// creating the missing ones. Returns the id of the last folder. Used by imports, which keep the source folders.
/// </summary>
public sealed record EnsureMediaFolderPathCommand(IReadOnlyList<string> Names) : ICommand<Result<int>>;

using Media.Application.Contracts;

namespace Media.Application.Files.SearchFiles;

/// <param name="FolderId">Null searches the whole library.</param>
/// <param name="IncludeSubfolders">With a folder: also its subfolders.</param>
/// <param name="Search">Part of the photo name, case-insensitive.</param>
public sealed record SearchMediaFilesQuery(int? FolderId, bool IncludeSubfolders, string? Search, int Page, int PageSize)
    : IQuery<Result<MediaFilePage>>
{
    public const int MaxPageSize = 100;
}

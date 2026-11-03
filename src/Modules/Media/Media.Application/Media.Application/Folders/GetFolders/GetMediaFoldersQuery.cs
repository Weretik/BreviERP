using Media.Application.Contracts;

namespace Media.Application.Folders.GetFolders;

/// <summary>All folders as a flat list; the Admin builds the tree from ParentId.</summary>
public sealed record GetMediaFoldersQuery : IQuery<Result<IReadOnlyList<MediaFolderResponse>>>;

using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Queries.GetMediaFolders;

/// <summary>All folders as a flat list; the Admin builds the tree from ParentId.</summary>
public sealed record GetMediaFoldersQuery : IQuery<Result<IReadOnlyList<MediaFolderResponse>>>;

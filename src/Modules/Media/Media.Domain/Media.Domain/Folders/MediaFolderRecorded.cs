using BuildingBlocks.Domain.Events;

namespace Media.Domain.Folders;

public enum MediaFolderAction
{
    Created = 1,
    Renamed = 2,
    Moved = 3,
    Deleted = 4
}

/// <summary>A staff member created, renamed, moved or deleted a photo folder (ACT-001); folders made for an upload
/// path raise nothing.</summary>
public sealed record MediaFolderRecorded(MediaFolder Folder, MediaFolderAction Action, string? NameBefore = null)
    : IDomainEvent;

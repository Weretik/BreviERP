using BuildingBlocks.Domain.Events;

namespace Media.Domain.Files;

public enum MediaFileAction
{
    Uploaded = 1,
    Changed = 2,
    Moved = 3,
    Deleted = 4
}

/// <summary>A staff member uploaded, changed, moved or deleted a photo (ACT-001); a change keeps the name and alt
/// text before it. Imported photos (with a source key) raise nothing when created.</summary>
public sealed record MediaFileRecorded(
    MediaFile File,
    MediaFileAction Action,
    string? NameBefore = null,
    string? AltBefore = null) : IDomainEvent;

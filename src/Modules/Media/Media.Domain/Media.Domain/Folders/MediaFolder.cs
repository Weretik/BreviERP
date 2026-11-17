using BuildingBlocks.Domain.Entity;
using BuildingBlocks.Domain.Events;
using BuildingBlocks.Domain.Results;

namespace Media.Domain.Folders;

/// <summary>A folder of the photo library. Folders exist only in the database: moving a folder or a photo never moves files on the CDN.</summary>
public sealed class MediaFolder : DomainEventSource, IAggregateRoot
{
    public const int NameMaxLength = 120;

    private MediaFolder() { }

    private MediaFolder(string name, int? parentId, DateTimeOffset now)
    {
        Name = name;
        ParentId = parentId;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>A folder a staff member made; «Журнал дій» lists it.</summary>
    public static DomainResult<MediaFolder> Create(string name, int? parentId, DateTimeOffset now)
    {
        var folder = CreateForPath(name, parentId, now);
        if (folder.IsSuccess) folder.Value.Raise(new MediaFolderRecorded(folder.Value, MediaFolderAction.Created));
        return folder;
    }

    /// <summary>A folder of an upload path (such as a product's photos), made on the way; not listed.</summary>
    public static DomainResult<MediaFolder> CreateForPath(string name, int? parentId, DateTimeOffset now)
    {
        var normalized = MediaNames.Normalize(name, NameMaxLength);
        return normalized.IsFailure ? normalized.Error! : new MediaFolder(normalized.Value, parentId, now);
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;

    /// <summary>Null for a top-level folder.</summary>
    public int? ParentId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public DomainResult Rename(string name, DateTimeOffset now)
    {
        var normalized = MediaNames.Normalize(name, NameMaxLength);
        if (normalized.IsFailure) return normalized.Error!;
        if (normalized.Value != Name) Raise(new MediaFolderRecorded(this, MediaFolderAction.Renamed, Name));
        Name = normalized.Value;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    /// <summary>The caller checks the target with <see cref="MediaFolderTree.CanMove"/> first.</summary>
    public DomainResult MoveTo(int? parentId, DateTimeOffset now)
    {
        if (parentId == Id) return MediaErrors.FolderOwnParent();
        if (parentId != ParentId) Raise(new MediaFolderRecorded(this, MediaFolderAction.Moved));
        ParentId = parentId;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    /// <summary>Tells «Журнал дій» that the folder is being deleted; call it before removing it.</summary>
    public void MarkDeleted() => Raise(new MediaFolderRecorded(this, MediaFolderAction.Deleted));
}

namespace Media.Domain.Folders;

/// <summary>A folder of the photo library. Folders exist only in the database: moving a folder or a photo never moves files on the CDN.</summary>
public sealed class MediaFolder
{
    public const int NameMaxLength = 120;

    private MediaFolder() { }

    public MediaFolder(string name, int? parentId, DateTimeOffset now)
    {
        Name = MediaNames.Normalize(name, NameMaxLength);
        ParentId = parentId;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = null!;

    /// <summary>Null for a top-level folder.</summary>
    public int? ParentId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void Rename(string name, DateTimeOffset now)
    {
        Name = MediaNames.Normalize(name, NameMaxLength);
        UpdatedAt = now;
    }

    /// <summary>The caller checks the target with <see cref="MediaFolderTree.CanMove"/> first.</summary>
    public void MoveTo(int? parentId, DateTimeOffset now)
    {
        if (parentId == Id) throw new InvalidOperationException("A folder cannot be its own parent.");
        ParentId = parentId;
        UpdatedAt = now;
    }
}

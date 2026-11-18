namespace BuildingBlocks.Domain.Identifiers.Products;

/// <summary>
/// A photo of the Media library. Media owns it; Catalog, Content and Crm keep it on their rows. Shared kernel (ADR-0020).
/// </summary>
public readonly record struct MediaFileId(int Value) : IComparable<MediaFileId>
{
    public int CompareTo(MediaFileId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static MediaFileId? FromNullable(int? value) => value is { } id ? new MediaFileId(id) : null;

    public override string ToString() => Value.ToString();
}

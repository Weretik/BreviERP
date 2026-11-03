namespace Catalog.Domain.Media.ValueObjects;

/// <summary>Id of a photo in the Media module library; Catalog keeps it as a plain value without a database relation.</summary>
public readonly record struct MediaFileId
{
    public int Value { get; }

    private MediaFileId(int value)
    {
        Value = value;
    }

    public static MediaFileId Create(int value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Media file id must be positive.");

        return new MediaFileId(value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(MediaFileId id) => id.Value;
}

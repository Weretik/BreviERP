namespace Catalog.Domain.Media.ValueObjects;

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

    internal static MediaFileId CreateTemporary(int value)
    {
        if (value >= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Temporary media file id must be negative.");

        return new MediaFileId(value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(MediaFileId id) => id.Value;
}

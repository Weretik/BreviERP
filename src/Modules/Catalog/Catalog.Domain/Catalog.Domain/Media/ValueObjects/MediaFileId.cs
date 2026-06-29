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

        if (value > 1_000_000_000)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Media file id is too large.");

        return new MediaFileId(value);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator int(MediaFileId id) => id.Value;
}

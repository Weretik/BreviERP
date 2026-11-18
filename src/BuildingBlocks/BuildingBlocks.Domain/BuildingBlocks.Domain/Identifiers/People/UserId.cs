namespace BuildingBlocks.Domain.Identifiers.People;

/// <summary>
/// A staff account of Identity (an ASP.NET Identity user). Identity keeps no aggregate of its own, so the id lives in
/// the shared kernel and every module that records who did something uses it (ADR-0020).
/// </summary>
public readonly record struct UserId(int Value) : IComparable<UserId>
{
    public int CompareTo(UserId other) => Value.CompareTo(other.Value);

    /// <summary>The id of a value that may be missing, for a value that comes from a request or another module.</summary>
    public static UserId? FromNullable(int? value) => value is { } id ? new UserId(id) : null;

    public override string ToString() => Value.ToString();
}

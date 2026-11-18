namespace BuildingBlocks.Domain;

/// <summary>
/// The request that created a row (idempotency rules 2): the client's <c>requestId</c> and a hash of what it asked for.
/// A repeat with the same id finds the row; <see cref="Matches"/> tells whether it asked for the same thing.
/// </summary>
public readonly record struct RequestKey
{
    /// <summary>A SHA-256 hash in hex.</summary>
    public const int HashLength = 64;

    private RequestKey(Guid id, string hash)
    {
        Id = id;
        Hash = hash;
    }

    public Guid Id { get; }
    public string Hash { get; }

    public static RequestKey Create(Guid id, string hash)
    {
        if (id == Guid.Empty) throw new ArgumentException("A request id is not empty.", nameof(id));
        if (hash is not { Length: HashLength }) throw new ArgumentException($"A request hash has {HashLength} characters.", nameof(hash));
        return new RequestKey(id, hash);
    }

    /// <summary>
    /// Whether the row saved under this id was made from the same data. A row saved before hashes were kept has none
    /// and matches, as it did then.
    /// </summary>
    public bool Matches(string? savedHash) => savedHash is null || string.Equals(savedHash, Hash, StringComparison.Ordinal);
}

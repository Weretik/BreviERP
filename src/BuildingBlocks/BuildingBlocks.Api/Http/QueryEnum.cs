
namespace BuildingBlocks.Api.Http;

/// <summary>
/// An enum in the query string, read like MVC did: the name in any letter case or the number. Minimal API binds a bare
/// enum by its exact name only, so <c>?filter=needsReview</c> would become 400.
/// </summary>
public readonly record struct QueryEnum<T>(T Value) where T : struct, Enum
{
    public static bool TryParse(string? value, IFormatProvider? provider, out QueryEnum<T> result)
    {
        if (Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
        {
            result = new QueryEnum<T>(parsed);
            return true;
        }

        result = default;
        return false;
    }

    public static implicit operator T(QueryEnum<T> query) => query.Value;
}

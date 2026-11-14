
namespace BuildingBlocks.Domain.Results;

/// <summary>What kind of failure an <see cref="Error"/> is; the one Api mapper turns it into an HTTP status.</summary>
public enum ErrorType
{
    /// <summary>The input breaks a rule (400).</summary>
    Validation,

    /// <summary>The thing does not exist (404).</summary>
    NotFound,

    /// <summary>The current state does not allow it: a duplicate, a stale version, a used record (409).</summary>
    Conflict,

    /// <summary>The user may not do it (403).</summary>
    Forbidden,

    /// <summary>Something failed that the user cannot fix (500).</summary>
    Unexpected
}

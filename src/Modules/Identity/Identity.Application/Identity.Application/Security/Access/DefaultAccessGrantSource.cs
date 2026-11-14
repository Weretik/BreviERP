using Identity.Domain.Authorization;

namespace Identity.Application.Security.Access;

/// <summary>
/// The catalogue defaults only: used when the Identity database is not registered (API tests without a database).
/// Identity registers the source that reads saved rights.
/// </summary>
public sealed class DefaultAccessGrantSource : IAccessGrantSource
{
    public Task<IReadOnlyDictionary<string, AccessAction>> GrantsOfAsync(
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
        => Task.FromResult(roles.Any(AccessCatalog.IsLocked)
            ? AccessCatalog.Everything()
            : AccessCatalog.Combine(roles.Select(AccessCatalog.DefaultsOf)));
}

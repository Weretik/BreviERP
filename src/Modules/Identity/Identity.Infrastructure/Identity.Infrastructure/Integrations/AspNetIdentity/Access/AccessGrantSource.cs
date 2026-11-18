using Identity.Application.Security.Access;
using Identity.Domain.Authorization;
using System.ComponentModel;
using BuildingBlocks.Application.Caching;
using Identity.Infrastructure.Persistence;

namespace Identity.Infrastructure.Integrations.AspNetIdentity.Access;

/// <summary>
/// Rights from <c>identity.role_access</c>, read on every admin request, so kept in the read cache (ADR-0021): a save on
/// this instance drops <see cref="CacheTag"/> at once, other instances see it within 30 seconds.
/// </summary>
public sealed class AccessGrantSource(AppIdentityDbContext dbContext, IReadCache cache) : IAccessGrantSource
{
    internal const string CacheTag = "identity:role-access";
    private static readonly ReadCacheEntry Entry =
        ReadCacheEntry.Create("identity.role-access", CacheTag, TimeSpan.FromSeconds(30));

    public async Task<IReadOnlyDictionary<string, AccessAction>> GrantsOfAsync(
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        if (roles.Any(AccessCatalog.IsLocked)) return AccessCatalog.Everything();

        var saved = await SavedAsync(cancellationToken);
        return AccessCatalog.Combine(roles.Select(role => saved.TryGetValue(role, out var stored) ? stored : AccessCatalog.DefaultsOf(role)));
    }

    /// <summary>Saved rights by role name.</summary>
    internal async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, AccessAction>>> SavedAsync(
        CancellationToken cancellationToken)
        => (await cache.GetOrCreateAsync(Entry, ReadSavedAsync, cancellationToken)).ByRole;

    private async ValueTask<SavedRoleAccess> ReadSavedAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.RoleAccess.AsNoTracking()
            .Select(x => new { x.Role.Name, x.Grants })
            .ToListAsync(cancellationToken);
        var saved = rows.Where(x => x.Name is not null)
            .ToDictionary(x => x.Name!, x => Parse(x.Grants, x.Name!), StringComparer.Ordinal);
        return new SavedRoleAccess(saved);
    }

    /// <summary>The cached rights by role name; built once and never changed.</summary>
    [ImmutableObject(true)]
    private sealed record SavedRoleAccess(IReadOnlyDictionary<string, IReadOnlyDictionary<string, AccessAction>> ByRole);

    /// <summary>
    /// Reads stored JSON of <paramref name="role"/>; pages the catalogue no longer has and actions a page lacks are dropped.
    /// A page with <see cref="AccessArea.DefaultsForSavedRoles"/> that the save does not mention gets the role's default.
    /// </summary>
    internal static IReadOnlyDictionary<string, AccessAction> Parse(string json, string role)
    {
        var stored = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
        var result = new Dictionary<string, AccessAction>(StringComparer.Ordinal);
        foreach (var (key, value) in stored)
        {
            var area = AccessCatalog.Find(key);
            var actions = area is null ? AccessAction.None : (AccessAction)value & area.Actions;
            if (actions != AccessAction.None) result[key] = actions;
        }

        foreach (var area in AccessCatalog.Areas.Where(x => x.DefaultsForSavedRoles && !stored.ContainsKey(x.Key)))
        {
            var actions = area.Defaults.GetValueOrDefault(role);
            if (actions != AccessAction.None) result[area.Key] = actions;
        }

        return result;
    }

    /// <summary>Granted pages, plus an explicit 0 for unticked pages whose defaults reach saved roles.</summary>
    internal static string Serialize(IReadOnlyDictionary<string, AccessAction> grants) =>
        JsonSerializer.Serialize(AccessCatalog.Areas
            .Select(area => (area.Key, Actions: grants.GetValueOrDefault(area.Key), area.DefaultsForSavedRoles))
            .Where(x => x.Actions != AccessAction.None || x.DefaultsForSavedRoles)
            .ToDictionary(x => x.Key, x => (int)x.Actions, StringComparer.Ordinal));
}

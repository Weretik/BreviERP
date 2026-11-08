using Identity.Application.Security.Access;
using Identity.Domain.Authorization;
using Identity.Infrastructure.DataBase;
using Microsoft.Extensions.Caching.Memory;

namespace Identity.Infrastructure.Services.Access;

/// <summary>
/// Rights from <c>identity.role_access</c>, kept in memory for a short while: a save on this instance drops them at
/// once, other instances see it within <see cref="CacheFor"/>.
/// </summary>
public sealed class AccessGrantSource(AppIdentityDbContext dbContext, IMemoryCache cache) : IAccessGrantSource
{
    internal const string CacheKey = "identity:role-access";
    private static readonly TimeSpan CacheFor = TimeSpan.FromSeconds(30);

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
    {
        if (cache.TryGetValue(CacheKey, out IReadOnlyDictionary<string, IReadOnlyDictionary<string, AccessAction>>? cached)
            && cached is not null)
        {
            return cached;
        }

        var rows = await dbContext.RoleAccess.AsNoTracking()
            .Select(x => new { x.Role.Name, x.Grants })
            .ToListAsync(cancellationToken);
        var saved = rows.Where(x => x.Name is not null)
            .ToDictionary(x => x.Name!, x => Parse(x.Grants), StringComparer.Ordinal);
        cache.Set(CacheKey, (IReadOnlyDictionary<string, IReadOnlyDictionary<string, AccessAction>>)saved, CacheFor);
        return saved;
    }

    /// <summary>Reads stored JSON; pages the catalogue no longer has and actions a page lacks are dropped.</summary>
    internal static IReadOnlyDictionary<string, AccessAction> Parse(string json)
    {
        var stored = JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? [];
        var result = new Dictionary<string, AccessAction>(StringComparer.Ordinal);
        foreach (var (key, value) in stored)
        {
            var area = AccessCatalog.Find(key);
            var actions = area is null ? AccessAction.None : (AccessAction)value & area.Actions;
            if (actions != AccessAction.None) result[key] = actions;
        }

        return result;
    }

    internal static string Serialize(IReadOnlyDictionary<string, AccessAction> grants) =>
        JsonSerializer.Serialize(grants.Where(x => x.Value != AccessAction.None)
            .ToDictionary(x => x.Key, x => (int)x.Value, StringComparer.Ordinal));
}

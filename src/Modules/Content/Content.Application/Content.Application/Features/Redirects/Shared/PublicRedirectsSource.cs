using Content.Application.Abstractions;
using BuildingBlocks.Application.Caching;

namespace Content.Application.Features.Redirects.Shared;

public interface IPublicRedirectsSource
{
    Task<IReadOnlyList<PublicRedirect>> GetAsync(CancellationToken cancellationToken);
}

/// <summary>
/// What the public redirect API answers from (CNT-005 R-06): every redirect with its chain followed to the final page,
/// so a visitor gets one 301. A chain longer than <see cref="MaxHops"/> or one that comes back to itself is left out.
/// Read through <see cref="PublicSiteCache"/>, so an anonymous caller cannot make the server read the table on each request.
/// </summary>
public sealed class PublicRedirectsSource(IContentDbContext db, IReadCache cache) : IPublicRedirectsSource
{
    public const int MaxHops = 5;
    private static readonly ReadCacheEntry Entry = PublicSiteCache.Entry("public-redirects");

    public async Task<IReadOnlyList<PublicRedirect>> GetAsync(CancellationToken cancellationToken)
        => (await cache.GetOrCreateAsync(Entry, ReadAsync, cancellationToken)).Items;

    private async ValueTask<PublicRedirectsSnapshot> ReadAsync(CancellationToken cancellationToken)
    {
        var rows = await db.SiteRedirects.AsNoTracking().OrderBy(x => x.FromPath)
            .Select(x => new { x.FromPath, x.ToPath }).ToListAsync(cancellationToken);
        var next = rows.ToDictionary(x => x.FromPath, x => x.ToPath, StringComparer.Ordinal);
        var items = new List<PublicRedirect>(rows.Count);
        foreach (var row in rows)
        {
            if (Final(row.FromPath, next) is { } to) items.Add(new PublicRedirect(row.FromPath, to));
        }

        return new PublicRedirectsSnapshot(items);
    }

    /// <summary>The page a redirect ends on, or null for a loop or a chain that is too long.</summary>
    public static string? Final(string from, IReadOnlyDictionary<string, string> next)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal) { from };
        var current = next[from];
        for (var hop = 1; next.TryGetValue(current, out var further); hop++)
        {
            if (hop >= MaxHops || !visited.Add(current)) return null;
            current = further;
        }

        return visited.Contains(current) ? null : current;
    }
}

/// <summary>The cached list of redirects.</summary>
[System.ComponentModel.ImmutableObject(true)]
public sealed record PublicRedirectsSnapshot(IReadOnlyList<PublicRedirect> Items);

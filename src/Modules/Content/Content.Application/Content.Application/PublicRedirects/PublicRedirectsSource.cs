using Content.Application.Abstractions;
using Content.Application.Contracts;
using Microsoft.Extensions.Caching.Memory;

namespace Content.Application.PublicRedirects;

public interface IPublicRedirectsSource
{
    Task<IReadOnlyList<PublicRedirect>> GetAsync(CancellationToken cancellationToken);
}

/// <summary>
/// What the public redirect API answers from (CNT-005 R-06): every redirect with its chain followed to the final page,
/// so a visitor gets one 301. A chain longer than <see cref="MaxHops"/> or one that comes back to itself is left out.
/// Kept for <see cref="Lifetime"/>, so an anonymous caller cannot make the server read the table on each request.
/// </summary>
public sealed class PublicRedirectsSource(IContentDbContext db, IMemoryCache cache) : IPublicRedirectsSource
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    public const int MaxHops = 5;
    private const string Key = "content:public-redirects";

    public async Task<IReadOnlyList<PublicRedirect>> GetAsync(CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key, out IReadOnlyList<PublicRedirect>? cached) && cached is not null) return cached;
        var rows = await db.SiteRedirects.AsNoTracking().OrderBy(x => x.FromPath)
            .Select(x => new { x.FromPath, x.ToPath }).ToListAsync(cancellationToken);
        var next = rows.ToDictionary(x => x.FromPath, x => x.ToPath, StringComparer.Ordinal);
        var items = new List<PublicRedirect>(rows.Count);
        foreach (var row in rows)
        {
            if (Final(row.FromPath, next) is { } to) items.Add(new PublicRedirect(row.FromPath, to));
        }

        cache.Set(Key, (IReadOnlyList<PublicRedirect>)items, Lifetime);
        return items;
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

using BuildingBlocks.Domain.Results;
using System.Text.Json.Serialization;

namespace Content.Domain.Redirects;

/// <summary>Written by name in JSON.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SiteRedirectSource>))]
public enum SiteRedirectSource
{
    /// <summary>Added by staff.</summary>
    Manual = 1,

    /// <summary>Made by the transfer of old-site addresses.</summary>
    OldSite = 2,

    /// <summary>Made when staff changed the address of a product, a category or an article.</summary>
    SlugChange = 3,
}

/// <summary>
/// A permanent (301) redirect of the public site (CNT-005): a visitor or a search engine that asks for
/// <see cref="FromPath"/> is sent to <see cref="ToPath"/>, a page of the new site. Both paths are normalized by
/// <see cref="SiteRedirectRules.Normalize"/>.
/// </summary>
public sealed class SiteRedirect
{
    private SiteRedirect() { }

    public static DomainResult<SiteRedirect> Create(string fromPath, string toPath, SiteRedirectSource source,
        string? note, string updatedBy, DateTimeOffset now)
    {
        var redirect = new SiteRedirect();
        var changed = redirect.Change(fromPath, toPath, note, source, updatedBy, now);
        return changed.IsFailure ? changed.Error! : redirect;
    }

    public int Id { get; private set; }

    public string FromPath { get; private set; } = null!;

    public string ToPath { get; private set; } = null!;

    public SiteRedirectSource Source { get; private set; }

    /// <summary>What the old address was, for staff: «Товар «Костюм Верде»».</summary>
    public string? Note { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    /// <summary>Changes the addresses; an edit by staff makes the redirect theirs.</summary>
    public DomainResult Change(string fromPath, string toPath, string? note, SiteRedirectSource source,
        string updatedBy, DateTimeOffset now)
    {
        if (SiteRedirectRules.Normalize(fromPath) != fromPath) return SiteRedirectErrors.InvalidFromPath();
        if (SiteRedirectRules.Normalize(toPath) != toPath || !SiteRedirectRules.IsSitePath(toPath))
            return SiteRedirectErrors.InvalidToPath();
        if (fromPath == toPath) return SiteRedirectErrors.ToItself();
        FromPath = fromPath;
        ToPath = toPath;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        Source = source;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
        return DomainResult.Success();
    }
}

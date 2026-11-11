using System.Text.Json.Serialization;

namespace Content.Application.Contracts;

/// <summary>Where a page of the site comes from in «Контент».</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SeoArea>))]
public enum SeoArea
{
    Product,
    Category,
    Article,
    Page,
}

/// <summary>What a page shown on the site misses for search (CNT-008 R-02).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SeoProblemKind>))]
public enum SeoProblemKind
{
    /// <summary>No SEO title: the site takes the name.</summary>
    NoTitle,

    /// <summary>No SEO description: search engines pick a piece of the page.</summary>
    NoDescription,

    /// <summary>Longer than search results show in full.</summary>
    TitleTooLong,

    DescriptionTooLong,

    /// <summary>Another page of the site has the same title in this language.</summary>
    DuplicateTitle,

    /// <summary>No Russian texts: the Russian page shows the Ukrainian ones.</summary>
    NoRussian,
}

/// <summary>Site-wide settings that are not filled (CNT-008 R-03).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<SeoSiteProblem>))]
public enum SeoSiteProblem
{
    NoAnalytics,
    NoGoogleVerification,
    NoBingVerification,
    NoSharePicture,
}

/// <param name="Language"><c>uk</c> or <c>ru</c>; null for a problem of the whole page.</param>
public sealed record SeoProblem(SeoProblemKind Kind, string? Language);

/// <param name="Id">The product, category or article id, or the page key.</param>
/// <param name="Path">The address on the site without the language.</param>
public sealed record SeoIssue(SeoArea Area, string Id, string Name, string Path, IReadOnlyList<SeoProblem> Problems);

public sealed record SeoAreaSummary(SeoArea Area, int OnSite, int WithProblems);

/// <summary>«Контент → SEO-огляд»: what the site misses for search, only problems (CNT-008).</summary>
public sealed record SeoOverviewResponse(
    IReadOnlyList<SeoSiteProblem> SiteProblems,
    IReadOnlyList<SeoAreaSummary> Areas,
    IReadOnlyList<SeoIssue> Issues);

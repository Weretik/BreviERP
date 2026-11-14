using System.Text.Json.Serialization;

namespace Content.Application.Features.SeoOverview.Shared;

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

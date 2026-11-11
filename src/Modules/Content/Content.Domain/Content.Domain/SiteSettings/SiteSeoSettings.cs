using System.Text.RegularExpressions;

namespace Content.Domain.SiteSettings;

/// <summary>
/// Site-wide SEO settings of the public site (CNT-006): the ids of the analytics services, the verification codes of
/// the search consoles and the picture shown when a page without its own picture is shared. One row; staff connect
/// the services themselves, the site only places the codes.
/// </summary>
public sealed partial class SiteSeoSettings
{
    public const int SingletonId = 1;
    public const int VerificationMaxLength = 100;

    private SiteSeoSettings() { }

    public SiteSeoSettings(string updatedBy, DateTimeOffset now)
    {
        Id = SingletonId;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public int Id { get; private set; }

    /// <summary>Google Analytics 4 measurement id, <c>G-XXXXXXX</c>.</summary>
    public string? GoogleAnalyticsId { get; private set; }

    /// <summary>Google Tag Manager container id, <c>GTM-XXXXXX</c>.</summary>
    public string? GoogleTagManagerId { get; private set; }

    /// <summary>The <c>content</c> of the meta tag <c>google-site-verification</c> from Search Console.</summary>
    public string? GoogleSiteVerification { get; private set; }

    /// <summary>The <c>content</c> of the meta tag <c>msvalidate.01</c> from Bing Webmaster Tools.</summary>
    public string? BingSiteVerification { get; private set; }

    /// <summary>The picture of the «Фото» library for sharing; the id of another module, no FK.</summary>
    public int? DefaultOgPhotoId { get; private set; }

    public int Revision { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public string UpdatedBy { get; private set; } = null!;

    public void Replace(string? googleAnalyticsId, string? googleTagManagerId, string? googleSiteVerification,
        string? bingSiteVerification, int? defaultOgPhotoId, string updatedBy, DateTimeOffset now)
    {
        GoogleAnalyticsId = Clean(googleAnalyticsId)?.ToUpperInvariant();
        GoogleTagManagerId = Clean(googleTagManagerId)?.ToUpperInvariant();
        GoogleSiteVerification = Clean(googleSiteVerification);
        BingSiteVerification = Clean(bingSiteVerification);
        if (GoogleAnalyticsId is not null && !IsAnalyticsId(GoogleAnalyticsId)) throw new ArgumentException("Invalid GA4 id.");
        if (GoogleTagManagerId is not null && !IsTagManagerId(GoogleTagManagerId)) throw new ArgumentException("Invalid GTM id.");
        if (GoogleSiteVerification is not null && !IsVerification(GoogleSiteVerification))
            throw new ArgumentException("Invalid Google verification.");
        if (BingSiteVerification is not null && !IsVerification(BingSiteVerification))
            throw new ArgumentException("Invalid Bing verification.");
        DefaultOgPhotoId = defaultOgPhotoId;
        Revision++;
        UpdatedBy = updatedBy;
        UpdatedAtUtc = now;
    }

    public static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static bool IsAnalyticsId(string value) => AnalyticsId().IsMatch(value.ToUpperInvariant());

    public static bool IsTagManagerId(string value) => TagManagerId().IsMatch(value.ToUpperInvariant());

    /// <summary>A code as the consoles give it: letters, digits, <c>-</c> and <c>_</c>, at most 100.</summary>
    public static bool IsVerification(string value) => Verification().IsMatch(value);

    [GeneratedRegex("^G-[A-Z0-9]{4,15}$")]
    private static partial Regex AnalyticsId();

    [GeneratedRegex("^GTM-[A-Z0-9]{4,12}$")]
    private static partial Regex TagManagerId();

    [GeneratedRegex("^[A-Za-z0-9_-]{8,100}$")]
    private static partial Regex Verification();
}

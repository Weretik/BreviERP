using BuildingBlocks.Domain.Exceptions;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public partial class Product
{
    public const int CodeMaxLength = 80;
    public const int CurrentSiteUrlMaxLength = 500;
    public const int QuoteDescriptionMaxLength = 2000;
    public const string VerifiedOnFormat = "dd.MM.yyyy";
    public static readonly IReadOnlyList<string> Seasons = ["Літо", "Зима"];

    /// <summary>The model ID (MOD-…) for sewing, the site article (sale SKU) for PPE.</summary>
    public string? Code => Type == ProductType.Ppe ? CatalogProfile?.SaleSku : ModelCode;

    /// <summary>Sets <see cref="Code"/>. PPE needs a code; a sewing code change bumps the composition version.</summary>
    public void SetCode(string? code, DateTimeOffset updatedAt)
    {
        var normalized = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (normalized == Code) return;
        if (Type == ProductType.Sewing)
        {
            SetModelCode(normalized, updatedAt);
            return;
        }
        if (normalized is null || normalized.Length > CodeMaxLength)
            throw new DomainException(ProductErrors.OverviewInvalid("PPE needs a site article up to 80 characters"));
        EnsureCatalogProfile().SetSaleSku(normalized);
        MarkAsUpdated(updatedAt);
    }

    /// <summary>
    /// Replaces the «Товари» page fields. A checked product keeps its date or gets <paramref name="today"/>; an
    /// unchecked one has no date.
    /// </summary>
    public void SetOverview(ProductOverviewDetails details, DateOnly today, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (details.SectionId is <= 0)
            throw new DomainException(ProductErrors.OverviewInvalid("Section ID must be positive"));
        var season = Normalize(details.Season);
        if (season is not null && (Type != ProductType.Sewing || !Seasons.Contains(season)))
            throw new DomainException(ProductErrors.OverviewInvalid("Season «Літо» or «Зима» is allowed only for a sewing product"));
        var url = Normalize(details.CurrentSiteUrl);
        if (url is not null && (url.Length > CurrentSiteUrlMaxLength || !IsWebUrl(url)))
            throw new DomainException(ProductErrors.OverviewInvalid("Current site link must be an http(s) address up to 500 characters"));
        var quoteDescription = Normalize(details.QuoteDescription);
        if (quoteDescription?.Length > QuoteDescriptionMaxLength)
            throw new DomainException(ProductErrors.OverviewInvalid("Quote description must be 2000 characters or less"));

        var profile = EnsureCatalogProfile();
        var verifiedOn = details.IsVerified
            ? Normalize(details.VerifiedOn) ?? profile.SourceReviewDate ?? today.ToString(VerifiedOnFormat, System.Globalization.CultureInfo.InvariantCulture)
            : null;
        profile.SetOverview(details.SectionId, season, details.IsActive, details.IsVerified, verifiedOn, url, quoteDescription);
        MarkAsUpdated(updatedAt);
    }

    private ProductCatalogProfile EnsureCatalogProfile() => CatalogProfile ??= ProductCatalogProfile.Create(Id, true);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsWebUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}

using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Errors;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Domain.Products.Entities;

public partial class Product
{
    public const int CodeMaxLength = 80;
    public const int CurrentSiteUrlMaxLength = 500;
    public const int QuoteDescriptionMaxLength = 2000;
    public static readonly IReadOnlyList<string> Seasons = ["Літо", "Зима"];

    /// <summary>The model ID (MOD-…) for sewing, the site article (sale SKU) for PPE.</summary>
    public string? Code => Type == ProductType.Ppe ? CatalogProfile?.SaleSku : ModelCode;

    /// <summary>Sets <see cref="Code"/>. PPE needs a code; a sewing code change bumps the composition version.</summary>
    public DomainResult SetCode(string? code, DateTimeOffset updatedAt)
    {
        var normalized = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        if (normalized == Code) return DomainResult.Success();
        if (Type == ProductType.Sewing) return SetModelCode(normalized, updatedAt);
        if (normalized is null || normalized.Length > CodeMaxLength)
            return ProductErrors.OverviewInvalid("Для ЗІЗ вкажіть артикул сайту до 80 символів.");
        EnsureCatalogProfile().SetSaleSku(normalized);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    /// <summary>Replaces the «Товари» page fields.</summary>
    public DomainResult SetOverview(ProductOverviewDetails details, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(details);
        if (details.SectionId is <= 0)
            return ProductErrors.OverviewInvalid("Розділ вказано неправильно.");
        var season = Normalize(details.Season);
        if (season is not null && (Type != ProductType.Sewing || !Seasons.Contains(season)))
            return ProductErrors.OverviewInvalid("Сезон «Літо» або «Зима» — лише для швейного товару.");
        var url = Normalize(details.CurrentSiteUrl);
        if (url is not null && (url.Length > CurrentSiteUrlMaxLength || !IsWebUrl(url)))
            return ProductErrors.OverviewInvalid("Посилання на поточний сайт — адреса http(s) до 500 символів.");
        var quoteDescription = Normalize(details.QuoteDescription);
        if (quoteDescription?.Length > QuoteDescriptionMaxLength)
            return ProductErrors.OverviewInvalid("Опис для КП — не більше 2000 символів.");

        EnsureCatalogProfile().SetOverview(details.SectionId, season, details.IsActive, url, quoteDescription);
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    private ProductCatalogProfile EnsureCatalogProfile() => CatalogProfile ??= ProductCatalogProfile.Create(Id, true);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsWebUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
}

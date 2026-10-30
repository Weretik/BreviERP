using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Crm.Application.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;

namespace Host.Api.Crm;

/// <summary>Resolves a reviewed featured model from one imported workbook snapshot.</summary>
public sealed class IntroductionSourceMappingReader(CatalogDbContext catalog)
    : IIntroductionSourceMappingReader
{
    public async Task<IntroductionSourceMapping?> ResolveAsync(
        string snapshotHash, string summerModelCode, string winterModelCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(snapshotHash) || snapshotHash.Length != 64 ||
            !snapshotHash.All(Uri.IsHexDigit) ||
            string.IsNullOrWhiteSpace(summerModelCode) || summerModelCode.Length > 200 ||
            string.IsNullOrWhiteSpace(winterModelCode) || winterModelCode.Length > 200 ||
            summerModelCode == winterModelCode)
            return null;

        var summer = await ResolveModelIdAsync(snapshotHash, summerModelCode, cancellationToken);
        var winter = await ResolveModelIdAsync(snapshotHash, winterModelCode, cancellationToken);
        if (summer is null || winter is null || summer == winter ||
            !await IsReviewedProductAsync(summer.Value, "Літо", cancellationToken) ||
            !await IsReviewedProductAsync(winter.Value, "Зима", cancellationToken))
            return null;

        return new IntroductionSourceMapping(summer.Value, winter.Value);
    }

    private async Task<int?> ResolveModelIdAsync(string snapshotHash, string modelCode,
        CancellationToken cancellationToken)
    {
        // The import provenance table has a unique (Dataset, SourceKey) key.
        var id = await catalog.Database.SqlQuery<long>($"""
            SELECT "TargetId" AS "Value"
            FROM imports."CatalogSourceMappings"
            WHERE "Dataset" = 'Model' AND "SourceKey" = {modelCode}
              AND "TargetType" = 'Product' AND "SnapshotHash" = {snapshotHash}
            """).SingleOrDefaultAsync(cancellationToken);
        return id is > 0 and <= int.MaxValue ? (int)id : null;
    }

    private async Task<bool> IsReviewedProductAsync(int id, string expectedSeason,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.Create(id);
        var profile = await catalog.Products.AsNoTracking()
            .Where(p => p.Id == productId && p.Type == ProductType.Sewing &&
                !p.IsDeleted && p.CatalogProfile != null &&
                p.CatalogProfile.IsActive && p.CatalogProfile.IsSourceReviewed &&
                p.CatalogProfile.Season == expectedSeason)
            .Select(p => p.CatalogProfile!.WebsiteUrl)
            .SingleOrDefaultAsync(cancellationToken);
        return IsPublicProductUrl(profile);
    }

    private static bool IsPublicProductUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var url) ||
            url.Scheme != Uri.UriSchemeHttps || url.UserInfo.Length > 0 ||
            url.AbsolutePath is "" or "/") return false;
        return url.Host.Equals("brevi.com.ua", StringComparison.OrdinalIgnoreCase) ||
            url.Host.EndsWith(".brevi.com.ua", StringComparison.OrdinalIgnoreCase);
    }
}

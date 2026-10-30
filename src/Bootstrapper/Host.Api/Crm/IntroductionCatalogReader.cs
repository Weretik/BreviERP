using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Quotes.Introductions;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace Host.Api.Crm;

public sealed class IntroductionCatalogReader(
    CatalogDbContext catalog,
    ReferenceDbContext reference,
    IQuoteCatalogPriceReader prices) : IIntroductionCatalogReader
{
    public async Task<IntroductionCatalogOffer?> ReadAsync(int productId, long articleId, CancellationToken cancellationToken)
    {
        if (productId <= 0 || articleId <= 0)
            return null;

        var productKey = ProductId.Create(productId);
        var articleKey = ArticleId.Create(articleId);
        var identity = await catalog.Products.AsNoTracking()
            .Where(p => p.Id == productKey && p.Type == ProductType.Sewing && !p.IsDeleted && p.CatalogProfile != null)
            .SelectMany(p => p.Articles.Where(a => a.Id == articleKey), (p, a) => new
            {
                p.Name,
                p.CatalogProfile!.Season,
                p.CatalogProfile.IsActive,
                p.CatalogProfile.IsSourceReviewed,
                p.CatalogProfile.WebsiteUrl,
                ArticleIsActive = a.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (identity is null)
            return null;

        var season = identity.Season switch
        {
            "Літо" => IntroductionSeason.Summer,
            "Зима" => IntroductionSeason.Winter,
            _ => (IntroductionSeason?)null
        };
        if (season is null)
            return null;

        var fabricIds = await catalog.Set<Catalog.Domain.Products.Entities.SewingMaterialLine>().AsNoTracking()
            .Where(line => line.ProductId == productKey && line.ArticleId == articleKey &&
                line.Kind == SewingMaterialKind.Fabric && line.FabricId != null)
            .Select(line => line.FabricId!.Value.Value)
            .Take(2)
            .ToArrayAsync(cancellationToken);
        string fabricName = "", composition = "", density = "";
        if (fabricIds.Length == 1)
        {
            var fabric = await reference.Fabrics.AsNoTracking()
                .Where(row => row.Id == FabricId.From(fabricIds[0]))
                .Select(row => new { row.Name, row.Composition, row.Density })
                .SingleOrDefaultAsync(cancellationToken);
            if (fabric is not null)
            {
                fabricName = fabric.Name;
                composition = fabric.Composition ?? "";
                density = fabric.Density ?? "";
            }
        }

        var price = await prices.ReadAsync("sewing", productId, articleId, cancellationToken);
        return new IntroductionCatalogOffer(season.Value, "made", identity.IsSourceReviewed,
            identity.IsActive && identity.ArticleIsActive && fabricIds.Length == 1,
            identity.Name, fabricName, composition, density, identity.WebsiteUrl ?? "", price);
    }
}

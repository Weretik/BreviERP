using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.SaveContentProduct;
using BuildingBlocks.Application;

namespace Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

public sealed class ImportOldSiteProductPagesHandler(
    IContentDbContext db, IContentCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<ImportOldSiteProductPagesCommand, Result<OldSiteImportResponse>>
{
    public const string NoProduct = "У «Товари» немає товару з таким посиланням на старий сайт.";
    public const string Duplicate = "Цей товар уже отримав сторінку з іншого рядка файлу.";
    public const string Empty = "На сторінці старого сайту немає текстів.";

    public async ValueTask<Result<OldSiteImportResponse>> Handle(ImportOldSiteProductPagesCommand command,
        CancellationToken cancellationToken)
    {
        var products = await catalog.GetAllAsync(cancellationToken);
        var byPath = new Dictionary<string, ContentCatalogProduct>(StringComparer.Ordinal);
        var byLastPart = new Dictionary<string, ContentCatalogProduct>(StringComparer.Ordinal);
        foreach (var product in products)
        {
            var path = OldSiteLinks.Path(product.OldSiteUrl);
            if (path.Length == 0) continue;
            byPath.TryAdd(path, product);
            byLastPart.TryAdd(OldSiteLinks.LastPart(product.OldSiteUrl), product);
        }

        var pages = await db.ProductPages.Include(x => x.Texts).ToDictionaryAsync(x => x.ProductId, cancellationToken);
        var slugs = pages.Values.Select(x => x.Slug).ToHashSet(StringComparer.Ordinal);
        var done = new HashSet<int>();
        var notMatched = new List<OldSiteImportNote>();
        var problems = new List<OldSiteImportNote>();
        int created = 0, updated = 0, kept = 0;
        var now = clock.GetUtcNow();

        foreach (var item in command.Items)
        {
            var name = item.Uk?.Name ?? item.Ru?.Name;
            var product = Match(item, byPath, byLastPart);
            if (product is null)
            {
                notMatched.Add(new OldSiteImportNote(item.Url, name, NoProduct));
                continue;
            }

            if (!done.Add(product.Id))
            {
                problems.Add(new OldSiteImportNote(item.Url, name, Duplicate));
                continue;
            }

            var texts = new Dictionary<string, ProductPageTextInput>();
            if (item.Uk is not null) texts[ProductPageRules.Ukrainian] = item.Uk;
            if (item.Ru is not null) texts[ProductPageRules.Russian] = item.Ru;
            if (texts.Count == 0)
            {
                problems.Add(new OldSiteImportNote(item.Url, name, Empty));
                continue;
            }

            if (pages.TryGetValue(product.Id, out var page))
            {
                if (page.Texts.Count > 0 && !command.Overwrite)
                {
                    kept++;
                    continue;
                }

                updated++;
            }
            else
            {
                var made = ProductPage.Create(product.Id, FreeSlug(item, product, slugs), command.UpdatedBy, now);
                if (made.IsFailure) return made.Error!.ToResult<OldSiteImportResponse>();
                page = made.Value;
                slugs.Add(page.Slug);
                if (!command.DryRun) db.ProductPages.Add(page);
                created++;
            }

            if (!command.DryRun)
            {
                var replaced = page.Replace(page.Slug, command.ShowOnSite || page.ShowOnSite, texts, command.UpdatedBy, now);
                if (replaced.IsFailure) return replaced.Error!.ToResult<OldSiteImportResponse>();
            }
        }

        if (!command.DryRun) await db.SaveChangesAsync(cancellationToken);
        return new OldSiteImportResponse(command.DryRun, command.Items.Count, created, updated, kept, notMatched, problems);
    }

    private static ContentCatalogProduct? Match(OldSiteProductPage item,
        Dictionary<string, ContentCatalogProduct> byPath, Dictionary<string, ContentCatalogProduct> byLastPart)
    {
        foreach (var url in new[] { item.Url, item.UrlRu })
        {
            var path = OldSiteLinks.Path(url);
            if (path.Length > 0 && byPath.TryGetValue(path, out var product)) return product;
        }

        foreach (var url in new[] { item.Url, item.UrlRu })
        {
            var last = OldSiteLinks.LastPart(url);
            if (last.Length > 0 && byLastPart.TryGetValue(last, out var product)) return product;
        }

        return null;
    }

    /// <summary>The old address when it fits the slug rule and is free, so old links can redirect one to one.</summary>
    private static string FreeSlug(OldSiteProductPage item, ContentCatalogProduct product, HashSet<string> taken)
    {
        var candidates = new[]
        {
            OldSiteLinks.LastPart(item.Url), ProductPageRules.SlugFrom(product.Name),
            ProductPageRules.SlugFrom($"{product.Name} {product.Id}"),
        };
        return candidates.FirstOrDefault(x => ProductPageRules.IsValidSlug(x) && !taken.Contains(x))
               ?? $"product-{product.Id}";
    }
}

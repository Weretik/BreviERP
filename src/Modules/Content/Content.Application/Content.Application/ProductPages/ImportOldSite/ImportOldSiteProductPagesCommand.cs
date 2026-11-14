using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.ProductPages.Save;
using Content.Application.ProductPages.Shared;
using Content.Domain.ProductPages;

namespace Content.Application.ProductPages.ImportOldSite;

/// <summary>A product page of the old site: its links in both languages and its texts.</summary>
public sealed record OldSiteProductPage(string Url, string? UrlRu, ProductPageTextInput? Uk, ProductPageTextInput? Ru);

/// <summary>
/// One-time transfer of old-site texts and SEO (CNT-002 SC-05). A page is matched to a Catalog product by the
/// old-site link of its card: the whole path in either language, then the last part of the path.
/// <paramref name="DryRun"/> only counts. <paramref name="Overwrite"/> replaces texts of pages that already have some;
/// without it they are kept. <paramref name="ShowOnSite"/> switches transferred pages on; it never switches a page off.
/// </summary>
public sealed record ImportOldSiteProductPagesCommand(
    bool DryRun,
    bool ShowOnSite,
    bool Overwrite,
    IReadOnlyList<OldSiteProductPage> Items,
    string UpdatedBy) : ICommand<Result<OldSiteImportResponse>>
{
    public const int MaxItems = 500;
    public const int UrlMaxLength = 500;
}

public sealed class ImportOldSiteProductPagesCommandValidator : AbstractValidator<ImportOldSiteProductPagesCommand>
{
    public ImportOldSiteProductPagesCommandValidator()
    {
        RuleFor(x => x.UpdatedBy).NotEmpty();
        RuleFor(x => x.Items).NotNull().Must(items => items is { Count: > 0 and <= ImportOldSiteProductPagesCommand.MaxItems })
            .WithMessage($"Файл має містити від 1 до {ImportOldSiteProductPagesCommand.MaxItems} сторінок.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Url).NotEmpty().MaximumLength(ImportOldSiteProductPagesCommand.UrlMaxLength);
            item.RuleFor(x => x.UrlRu).MaximumLength(ImportOldSiteProductPagesCommand.UrlMaxLength);
            item.RuleFor(x => x.Uk!).SetValidator(new ProductPageTextInputValidator("Українська")).When(x => x.Uk is not null);
            item.RuleFor(x => x.Ru!).SetValidator(new ProductPageTextInputValidator("Російська")).When(x => x.Ru is not null);
        }).When(x => x.Items is not null);
    }
}

public sealed class ImportOldSiteProductPagesCommandHandler(
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

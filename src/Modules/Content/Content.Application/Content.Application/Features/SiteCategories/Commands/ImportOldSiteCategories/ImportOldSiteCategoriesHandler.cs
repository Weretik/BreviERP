using Content.Application.Abstractions;
using Content.Domain.ProductPages;
using Content.Domain.SiteCategories;
using Content.Application.Features.Articles.Shared;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;
using Content.Application.Features.SiteCategories.Commands.SaveSiteCategory;

namespace Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;

public sealed class ImportOldSiteCategoriesHandler(
    IContentDbContext db, IContentCatalogReader catalog, TimeProvider clock)
    : ICommandHandler<ImportOldSiteCategoriesCommand, Result<OldSiteCategoryImportResponse>>
{
    public const string NoName = "У розділу немає назви українською.";
    public const string Duplicate = "Розділ із такою адресою вже є вище у файлі.";
    public const string NoParent = "Батьківського розділу немає ні у файлі, ні в адмінці.";
    public const string ParentIsSubcategory = "Батьківський розділ сам є підрозділом; дерево має два рівні.";
    public const string HasSubcategories = "Розділ уже має підрозділи, тому не може стати підрозділом.";
    public const string NoProduct = "У «Товари» немає товару з таким посиланням на старий сайт.";

    public async ValueTask<Result<OldSiteCategoryImportResponse>> Handle(ImportOldSiteCategoriesCommand command,
        CancellationToken cancellationToken)
    {
        var byPath = new Dictionary<string, int>(StringComparer.Ordinal);
        var byLastPart = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var product in await catalog.GetAllAsync(cancellationToken))
        {
            var path = OldSiteLinks.Path(product.OldSiteUrl);
            if (path.Length == 0) continue;
            byPath.TryAdd(path, product.Id);
            byLastPart.TryAdd(OldSiteLinks.LastPart(product.OldSiteUrl), product.Id);
        }

        var existing = await db.SiteCategories.Include(x => x.Texts).ToListAsync(cancellationToken);
        var bySlug = existing.ToDictionary(x => x.Slug, StringComparer.Ordinal);
        var withChildren = existing.Where(x => x.ParentId is not null).Select(x => x.ParentId!.Value).ToHashSet();
        // What the tree will look like: the address of the parent of every category, old and new.
        var parentOf = existing.ToDictionary(x => x.Slug,
            x => x.ParentId is null ? null : existing.First(parent => parent.Id == x.ParentId).Slug, StringComparer.Ordinal);
        var nextPlace = existing.GroupBy(x => x.ParentId is null ? string.Empty : parentOf[x.Slug]!)
            .ToDictionary(x => x.Key, x => x.Max(category => category.SortOrder), StringComparer.Ordinal);
        var links = (await db.SiteCategoryProducts.AsNoTracking().ToListAsync(cancellationToken))
            .GroupBy(x => x.CategoryId).ToDictionary(x => x.Key, x => x.ToList());

        var problems = new List<OldSiteImportNote>();
        var notMatched = new Dictionary<string, OldSiteImportNote>(StringComparer.Ordinal);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var accepted = new List<(OldSiteCategory Item, bool IsNew)>();
        int created = 0, updated = 0, kept = 0, productLinks = 0;

        // Categories of the top level first, so a subcategory finds its parent wherever it stands in the file.
        foreach (var item in command.Items.OrderBy(x => x.ParentSlug is null ? 0 : 1))
        {
            var name = ProductPageRules.Clean(item.Uk?.Name);
            if (!seen.Add(item.Slug)) problems.Add(Note(item, Duplicate));
            else if (name is null) problems.Add(Note(item, NoName));
            else if (item.ParentSlug is not null && !parentOf.ContainsKey(item.ParentSlug)) problems.Add(Note(item, NoParent));
            else if (item.ParentSlug is not null && parentOf[item.ParentSlug] is not null) problems.Add(Note(item, ParentIsSubcategory));
            else if (item.ParentSlug is not null && bySlug.TryGetValue(item.Slug, out var current) && withChildren.Contains(current.Id))
                problems.Add(Note(item, HasSubcategories));
            else
            {
                var isNew = !bySlug.ContainsKey(item.Slug);
                if (isNew) created++;
                else if (command.Overwrite) updated++;
                else kept++;
                if (isNew || command.Overwrite) parentOf[item.Slug] = item.ParentSlug;
                accepted.Add((item, isNew));
            }
        }

        var now = clock.GetUtcNow();
        // Categories are saved level by level (subcategories need their parents' ids) and links last: one transaction.
        await using var transaction = command.DryRun ? null : await db.BeginTransactionAsync(cancellationToken);
        foreach (var level in new[] { true, false })
        {
            foreach (var (item, isNew) in accepted.Where(x => (x.Item.ParentSlug is null) == level))
            {
                if (command.DryRun || !(isNew || command.Overwrite)) continue;
                int? parentId = item.ParentSlug is null ? null : bySlug[item.ParentSlug].Id;
                bySlug.TryGetValue(item.Slug, out var category);
                var place = category is not null && category.ParentId == parentId
                    ? category.SortOrder
                    : nextPlace[item.ParentSlug ?? string.Empty] = nextPlace.GetValueOrDefault(item.ParentSlug ?? string.Empty) + 1;
                if (category is null)
                {
                    var made = SiteCategory.Create(parentId, item.Slug, place, command.UpdatedBy, now);
                    if (made.IsFailure) return made.Error!.ToResult<OldSiteCategoryImportResponse>();
                    category = made.Value;
                    db.SiteCategories.Add(category);
                    bySlug[item.Slug] = category;
                }

                var texts = new Dictionary<string, SiteCategoryTextInput> { [ProductPageRules.Ukrainian] = item.Uk! };
                if (item.Ru is not null) texts[ProductPageRules.Russian] = item.Ru;
                var replaced = category.Replace(parentId, place, item.Slug, command.ShowOnSite || category.ShowOnSite,
                    texts, command.UpdatedBy, now);
                if (replaced.IsFailure) return replaced.Error!.ToResult<OldSiteCategoryImportResponse>();
            }

            // Subcategories need the ids of their categories.
            if (!command.DryRun) await db.SaveChangesAsync(cancellationToken);
        }

        foreach (var (item, _) in accepted)
        {
            var categoryId = bySlug.TryGetValue(item.Slug, out var category) ? category.Id : 0;
            var current = links.GetValueOrDefault(categoryId) ?? [];
            var inCategory = current.Select(x => x.ProductId).ToHashSet();
            var place = current.Count == 0 ? 0 : current.Max(x => x.SortOrder);
            foreach (var url in item.ProductUrls ?? [])
            {
                var productId = Match(url, byPath, byLastPart);
                if (productId is null)
                {
                    notMatched.TryAdd(OldSiteLinks.Path(url), new OldSiteImportNote(url, null, NoProduct));
                    continue;
                }

                if (!inCategory.Add(productId.Value)) continue;
                productLinks++;
                if (!command.DryRun) db.SiteCategoryProducts.Add(new SiteCategoryProduct(categoryId, productId.Value, ++place));
            }
        }

        if (transaction is not null)
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return new OldSiteCategoryImportResponse(command.DryRun, command.Items.Count, created, updated, kept, productLinks,
            notMatched.Values.ToArray(), problems);
    }

    private static OldSiteImportNote Note(OldSiteCategory item, string reason) =>
        new(item.Slug, item.Uk?.Name ?? item.Ru?.Name, reason);

    private static int? Match(string url, Dictionary<string, int> byPath, Dictionary<string, int> byLastPart)
    {
        var path = OldSiteLinks.Path(url);
        if (path.Length > 0 && byPath.TryGetValue(path, out var product)) return product;
        var last = OldSiteLinks.LastPart(url);
        return last.Length > 0 && byLastPart.TryGetValue(last, out product) ? product : null;
    }
}

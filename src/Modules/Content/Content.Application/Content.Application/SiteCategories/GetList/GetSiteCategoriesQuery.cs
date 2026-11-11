using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.SiteCategories.Shared;

namespace Content.Application.SiteCategories.GetList;

/// <summary>The tree of «Контент → Розділи»: every category followed by its subcategories (CNT-003 SC-01).</summary>
public sealed record GetSiteCategoriesQuery : IQuery<Result<SiteCategoryList>>;

public sealed class GetSiteCategoriesQueryHandler(IContentDbContext db)
    : IQueryHandler<GetSiteCategoriesQuery, Result<SiteCategoryList>>
{
    public async ValueTask<Result<SiteCategoryList>> Handle(GetSiteCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var categories = await db.SiteCategories.AsNoTracking().Include(x => x.Texts).ToListAsync(cancellationToken);
        var links = await db.SiteCategoryProducts.AsNoTracking().Select(x => new { x.CategoryId, x.ProductId })
            .ToListAsync(cancellationToken);
        var products = links.ToLookup(x => x.CategoryId, x => x.ProductId);
        var children = categories.Where(x => x.ParentId is not null).OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .ToLookup(x => x.ParentId!.Value);

        var rows = new List<SiteCategoryRow>(categories.Count);
        foreach (var category in categories.Where(x => x.ParentId is null).OrderBy(x => x.SortOrder).ThenBy(x => x.Id))
        {
            var own = children[category.Id].ToArray();
            var all = products[category.Id].Concat(own.SelectMany(x => products[x.Id])).Distinct().Count();
            rows.Add(SiteCategoryMapping.Row(category, null, all));
            rows.AddRange(own.Select(child => SiteCategoryMapping.Row(child, category, products[child.Id].Count())));
        }

        return new SiteCategoryList(rows);
    }
}

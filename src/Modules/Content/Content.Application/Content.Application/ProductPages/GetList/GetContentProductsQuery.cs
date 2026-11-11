using Content.Application.Abstractions;
using Content.Application.Contracts;
using Content.Application.ProductPages.Shared;

namespace Content.Application.ProductPages.GetList;

/// <summary>
/// One page of «Контент → Товари»: every Catalog product with the state of its site page. <paramref name="Search"/>
/// matches the code, the name or the slug; <paramref name="Type"/> is <c>Sewing</c> or <c>Ppe</c>;
/// <paramref name="OnSite"/> keeps products that are (true) or are not (false) on the site.
/// </summary>
public sealed record GetContentProductsQuery(
    int Page, int PageSize, string? Search = null, string? Type = null, int? SectionId = null, bool? OnSite = null)
    : IQuery<Result<ContentProductPage>>
{
    public const int MaxPageSize = 100;
    public const int SearchMaxLength = 200;
    public static readonly string[] Types = ["Sewing", "Ppe"];
}

public sealed class GetContentProductsQueryValidator : AbstractValidator<GetContentProductsQuery>
{
    public GetContentProductsQueryValidator()
    {
        RuleFor(x => x.Page).InclusiveBetween(1, 10_000);
        RuleFor(x => x.PageSize).InclusiveBetween(1, GetContentProductsQuery.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(GetContentProductsQuery.SearchMaxLength);
        RuleFor(x => x.Type).Must(type => type is null || GetContentProductsQuery.Types.Contains(type))
            .WithMessage("Тип: Sewing або Ppe.");
        RuleFor(x => x.SectionId).GreaterThan(0).When(x => x.SectionId is not null);
    }
}

public sealed class GetContentProductsQueryHandler(IContentDbContext db, IContentCatalogReader catalog)
    : IQueryHandler<GetContentProductsQuery, Result<ContentProductPage>>
{
    public async ValueTask<Result<ContentProductPage>> Handle(GetContentProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await catalog.GetAllAsync(cancellationToken);
        var pages = await db.ProductPages.AsNoTracking().Include(x => x.Texts)
            .ToDictionaryAsync(x => x.ProductId, cancellationToken);
        var categories = (await db.SiteCategoryProducts.AsNoTracking().Select(x => x.ProductId).ToListAsync(cancellationToken))
            .GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
        var search = query.Search?.Trim();

        var rows = products
            .Where(x => query.Type is null || x.Type == query.Type)
            .Where(x => query.SectionId is null || x.SectionId == query.SectionId)
            .Select(x => ContentProductMapping.Row(x, pages.GetValueOrDefault(x.Id), categories.GetValueOrDefault(x.Id)))
            .Where(x => query.OnSite is null || x.OnSite == query.OnSite)
            .Where(x => string.IsNullOrEmpty(search) || Matches(x, search))
            .ToArray();

        var items = rows.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToArray();
        return new ContentProductPage(items, query.Page, query.PageSize, rows.Length);
    }

    private static bool Matches(ContentProductRow row, string search) =>
        Has(row.Code, search) || Has(row.Name, search) || Has(row.Slug, search);

    private static bool Has(string? value, string search) =>
        value is not null && value.Contains(search, StringComparison.OrdinalIgnoreCase);
}

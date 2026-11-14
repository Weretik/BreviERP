using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Queries.GetSiteCategory;

/// <summary>The editor of one category or subcategory.</summary>
public sealed record GetSiteCategoryQuery(int Id) : IQuery<Result<SiteCategoryResponse>>;

public sealed class GetSiteCategoryValidator : AbstractValidator<GetSiteCategoryQuery>
{
    public GetSiteCategoryValidator() => RuleFor(x => x.Id).GreaterThan(0);
}

public sealed class GetSiteCategoryHandler(IContentDbContext db)
    : IQueryHandler<GetSiteCategoryQuery, Result<SiteCategoryResponse>>
{
    public async ValueTask<Result<SiteCategoryResponse>> Handle(GetSiteCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var category = await db.SiteCategories.AsNoTracking().Include(x => x.Texts)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (category is null) return Result.NotFound(SiteCategoryMessages.NotFound);
        return await SiteCategoryReads.DetailAsync(db, category, cancellationToken);
    }
}

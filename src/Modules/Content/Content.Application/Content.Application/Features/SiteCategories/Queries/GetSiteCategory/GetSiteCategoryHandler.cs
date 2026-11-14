using Content.Application.Abstractions;
using Content.Application.Features.SiteCategories.Shared;

namespace Content.Application.Features.SiteCategories.Queries.GetSiteCategory;

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

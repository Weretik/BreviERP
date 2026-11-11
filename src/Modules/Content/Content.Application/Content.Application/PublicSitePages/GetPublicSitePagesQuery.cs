using Content.Application.Contracts;

namespace Content.Application.PublicSitePages;

/// <summary>Search texts of the fixed pages of the public site (CNT-007 SC-03).</summary>
public sealed record GetPublicSitePagesQuery : IQuery<Result<PublicSitePagesResponse>>;

public sealed class GetPublicSitePagesQueryHandler(IPublicSitePagesSource source)
    : IQueryHandler<GetPublicSitePagesQuery, Result<PublicSitePagesResponse>>
{
    public async ValueTask<Result<PublicSitePagesResponse>> Handle(GetPublicSitePagesQuery query,
        CancellationToken cancellationToken) =>
        await source.GetAsync(cancellationToken);
}

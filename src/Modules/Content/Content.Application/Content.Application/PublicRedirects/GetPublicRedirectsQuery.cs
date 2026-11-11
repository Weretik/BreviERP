using Content.Application.Contracts;

namespace Content.Application.PublicRedirects;

/// <summary>Every redirect of the public site with its final page (CNT-005 SC-04).</summary>
public sealed record GetPublicRedirectsQuery : IQuery<Result<PublicRedirectList>>;

public sealed class GetPublicRedirectsQueryHandler(IPublicRedirectsSource source)
    : IQueryHandler<GetPublicRedirectsQuery, Result<PublicRedirectList>>
{
    public async ValueTask<Result<PublicRedirectList>> Handle(GetPublicRedirectsQuery query,
        CancellationToken cancellationToken) =>
        new PublicRedirectList(await source.GetAsync(cancellationToken));
}

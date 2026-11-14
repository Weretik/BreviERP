using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.GetPublicRedirects;

public sealed class GetPublicRedirectsHandler(IPublicRedirectsSource source)
    : IQueryHandler<GetPublicRedirectsQuery, Result<PublicRedirectList>>
{
    public async ValueTask<Result<PublicRedirectList>> Handle(GetPublicRedirectsQuery query,
        CancellationToken cancellationToken) =>
        new PublicRedirectList(await source.GetAsync(cancellationToken));
}

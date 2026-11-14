using Content.Domain.Redirects;

namespace Content.Application.Features.Redirects.Shared;

internal static class RedirectMapping
{
    public static SiteRedirectResponse Row(SiteRedirect x) =>
        new(x.Id, x.FromPath, x.ToPath, x.Source, x.Note, x.UpdatedAtUtc, x.UpdatedBy);
}

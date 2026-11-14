using Content.Application.Abstractions;
using Content.Domain.Redirects;
using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;

/// <summary>
/// Transfer of the old-site addresses (CNT-005 SC-03): every address gets a 301 to the page of the new site given
/// in the file or found by <see cref="OldSiteRedirectResolver"/>. <paramref name="DryRun"/> only reports.
/// An existing redirect is kept unless <paramref name="Overwrite"/> is on; a redirect staff added or edited by hand
/// is always kept. A repeated run changes nothing that is already right.
/// </summary>
public sealed record ImportOldSiteRedirectsCommand(
    bool DryRun,
    bool Overwrite,
    IReadOnlyList<OldSiteRedirectItem> Items,
    string UpdatedBy) : ICommand<Result<OldSiteRedirectImportResponse>>
{
    public const int MaxItems = 3_000;
    public const int AddressMaxLength = 2_000;
}

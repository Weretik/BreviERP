using Content.Domain.ProductPages;
using Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

namespace Content.Api.Endpoints.ProductPages.ImportOldSiteProductPages;

/// <summary>Body of <c>importOldSiteProductPages</c>; without <c>DryRun = false</c> nothing is written.</summary>
public sealed record ImportOldSiteProductPagesRequest(
    IReadOnlyList<OldSiteProductPage> Items, bool DryRun = true, bool ShowOnSite = false, bool Overwrite = false);

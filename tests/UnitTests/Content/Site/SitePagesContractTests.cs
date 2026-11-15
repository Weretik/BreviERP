using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Content.Domain.SitePages;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.SitePages.Queries.GetSitePages;
using Content.Application.Features.SitePages.Shared;
using Content.Api.Endpoints.SitePages.SaveSitePage;
using Identity.Domain.Authorization.Roles;

namespace UnitTests.Content.Site;

/// <summary>
/// «Сторінки сайту» and the public page texts match the generated contract docs/sdd/contracts/openapi.json and
/// content-public-site-pages.openapi.yaml (CNT-007, CNT-010).
/// </summary>
public sealed class SitePagesContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_site_pages_policy()
    {
        var controller = "SitePages";
        var policy = MappedEndpoints.Policy(Map, controller);

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentSitePages}"));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf(controller)));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf("PublicSitePages")));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_edit_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentSitePages)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Сторінки сайту"));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentSitePages], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentSitePages], Is.EqualTo(AccessAction.View));
        });
    }

    private static string[] OperationsOf(string controller) => MappedEndpoints.Of(Map, controller)
        .Select(x => x.Name!).ToArray();
}

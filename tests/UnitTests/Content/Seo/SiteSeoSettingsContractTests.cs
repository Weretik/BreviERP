using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.SiteSettings.Shared;
using Content.Api.Endpoints.SiteSettings.SaveSiteSeoSettings;
using Identity.Domain.Authorization.Roles;

namespace UnitTests.Content.Seo;

/// <summary>
/// «Налаштування SEO сайту» and the public site settings match the generated contract docs/sdd/contracts/openapi.json
/// and content-public-site-settings.openapi.yaml (CNT-006).
/// </summary>
public sealed class SiteSeoSettingsContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_seo_settings_policy()
    {
        var controller = "SiteSeoSettings";
        var policy = MappedEndpoints.Policy(Map, controller);

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentSeoSettings}"));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf(controller)));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf("PublicSiteSettings")));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_edit_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentSeoSettings)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Налаштування SEO сайту"));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentSeoSettings], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentSeoSettings], Is.EqualTo(AccessAction.View));
        });
    }

    private static string[] OperationsOf(string controller) => MappedEndpoints.Of(Map, controller)
        .Select(x => x.Name!).ToArray();
}

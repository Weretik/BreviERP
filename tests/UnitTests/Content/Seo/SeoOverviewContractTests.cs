using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.SeoOverview.Queries.GetSeoOverview;
using Content.Application.Features.SeoOverview.Shared;
using Identity.Domain.Authorization.Roles;

namespace UnitTests.Content.Seo;

/// <summary>«SEO-огляд» matches the generated contract docs/sdd/contracts/openapi.json (CNT-008).</summary>
public sealed class SeoOverviewContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Operation_is_in_the_contract_and_needs_the_seo_overview_policy()
    {
        var controller = "SeoOverview";
        var policy = MappedEndpoints.Policy(Map, controller);
        var operations = MappedEndpoints.Of(Map, controller)
            .Select(x => x.Name!);

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentSeoOverview}"));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_view_only_for_admin_and_manager()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentSeoOverview)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("SEO-огляд"));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentSeoOverview], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentSeoOverview], Is.EqualTo(AccessAction.View));
        });
    }
}

using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Content.Domain.SiteCategories;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.SiteCategories.Commands.ImportOldSiteCategories;
using Content.Application.Features.SiteCategories.Commands.SetSiteCategoryVisibility;
using Content.Application.Features.SiteCategories.Queries.GetSiteCategories;
using Content.Application.Features.SiteCategories.Shared;
using Content.Api.Endpoints.SiteCategories.ImportOldSiteCategories;
using Content.Api.Endpoints.SiteCategories.ReorderSiteCategories;
using Content.Api.Endpoints.SiteCategories.SetSiteCategoryVisibility;
using Content.Api.Endpoints.SiteCategories.Shared;
using Identity.Domain.Authorization.Roles;

namespace UnitTests.Content.Categories;

/// <summary>«Контент → Розділи» types and operations match the generated contract docs/sdd/contracts/openapi.json (CNT-003).</summary>
public sealed class SiteCategoriesContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_categories_policy()
    {
        var controller = "SiteCategories";
        var policy = MappedEndpoints.Policy(Map, controller);
        var operations = MappedEndpoints.Of(Map, controller)
            .Select(x => x.Name).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentCategories}"));
            Assert.That(operations, Has.Length.EqualTo(8));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(operations));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentCategories)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Розділи сайту"));
            Assert.That(area.Actions.HasFlag(AccessAction.Create | AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentCategories], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentCategories], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentCategories));
        });
    }
}

using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Content.Domain.Redirects;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects;
using Content.Application.Features.Redirects.Queries.GetPublicRedirects;
using Content.Application.Features.Redirects.Queries.GetSiteRedirects;
using Content.Application.Features.Redirects.Shared;
using Content.Api.Endpoints.Redirects.ImportOldSiteRedirects;
using Content.Api.Endpoints.Redirects.Shared;
using Identity.Domain.Authorization.Roles;
using Content.Application.Features.Redirects.Commands.ImportOldSiteRedirects.Results;

namespace UnitTests.Content.Seo;

/// <summary>
/// «Контент → Переадресації» and the public redirects match the generated contract docs/sdd/contracts/openapi.json
/// and content-public-redirects.openapi.yaml (CNT-005).
/// </summary>
public sealed class SiteRedirectsContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Source_is_written_by_name_as_in_the_contract()
    {
        Assert.That(JsonSerializer.Serialize(SiteRedirectSource.SlugChange), Is.EqualTo("\"SlugChange\""));
    }

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_content_redirects_policy()
    {
        var controller = "ContentRedirects";
        var policy = MappedEndpoints.Policy(Map, controller);

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentRedirects}"));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf(controller)));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf("PublicRedirects")));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_full_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentRedirects)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Переадресації"));
            Assert.That(area.Actions.HasFlag(AccessAction.Create | AccessAction.Edit | AccessAction.Delete), Is.True);
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentRedirects], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentRedirects], Is.EqualTo(AccessAction.View));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Accountant), Does.Not.ContainKey(AccessCatalog.ContentRedirects));
        });
    }

    private static string[] OperationsOf(string controller) => MappedEndpoints.Of(Map, controller)
        .Select(x => x.Name!).ToArray();
}

using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.SiteGalleries.Commands.SaveSiteGallery;
using Content.Application.Features.SiteGalleries.Commands.TransferSiteGallery;
using Content.Application.Features.SiteGalleries.Shared;
using Content.Api.Endpoints.SiteGalleries.Shared;
using Identity.Domain.Authorization.Roles;
using Content.Application.Features.SiteGalleries.Shared.Admin;
using Content.Application.Features.SiteGalleries.Shared.Public;

namespace UnitTests.Content.Site;

/// <summary>
/// «Наші роботи», «Сертифікати» and their public pictures match the generated contract docs/sdd/contracts/openapi.json
/// and content-public-galleries.openapi.yaml (CNT-011).
/// </summary>
public sealed class SiteGalleriesContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_its_page_policy()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Policy("SiteWorks"), Is.EqualTo($"access:{AccessCatalog.ContentWorks}"));
            Assert.That(Policy("SiteCertificates"), Is.EqualTo($"access:{AccessCatalog.ContentCertificates}"));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(
                OperationsOf("SiteWorks").Concat(OperationsOf("SiteCertificates"))));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf("PublicSiteGalleries")));
        });
    }

    [TestCase(AccessCatalog.ContentWorks, "Наші роботи")]
    [TestCase(AccessCatalog.ContentCertificates, "Сертифікати")]
    public void Page_is_in_access_rights_with_admin_edit_and_manager_view(string page, string title)
    {
        var area = AccessCatalog.Find(page)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo(title));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[page], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[page], Is.EqualTo(AccessAction.View));
        });
    }

    private static string? Policy(string controller) =>
        MappedEndpoints.Policy(Map, controller);

    private static string[] OperationsOf(string controller) => MappedEndpoints.Of(Map, controller)
        .Select(x => x.Name!).ToArray();
}

using Microsoft.AspNetCore.Routing;
using UnitTests.TestSupport;
using Content.Api.DependencyInjection;
using System.Text.Json;
using Identity.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;
using Content.Application.Features.SiteCompany.Shared;
using Content.Api.Endpoints.SiteCompany.SaveSiteCompany;
using Identity.Domain.Authorization.Roles;
using Content.Application.Features.SiteCompany.Shared.Company;
using Content.Application.Features.SiteCompany.Shared.Public;

namespace UnitTests.Content.Site;

/// <summary>
/// «Контакти та компанія» and the public company facts match the generated contract docs/sdd/contracts/openapi.json
/// and content-public-company.openapi.yaml (CNT-009).
/// </summary>
public sealed class SiteCompanyContractTests
{
    private static readonly Action<IEndpointRouteBuilder> Map = app => app.MapContentEndpoints();

    [Test]
    public void Every_operation_is_in_the_contract_and_needs_the_company_policy()
    {
        var controller = "SiteCompany";
        var policy = MappedEndpoints.Policy(Map, controller);

        Assert.Multiple(() =>
        {
            Assert.That(policy, Is.EqualTo($"access:{AccessCatalog.ContentCompany}"));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf(controller)));
            Assert.That(ApiContract.OperationIds, Is.SupersetOf(OperationsOf("PublicSiteCompany")));
        });
    }

    [Test]
    public void Page_is_in_access_rights_with_admin_edit_and_manager_view()
    {
        var area = AccessCatalog.Find(AccessCatalog.ContentCompany)!;
        Assert.Multiple(() =>
        {
            Assert.That(area.Title, Is.EqualTo("Контакти та компанія"));
            Assert.That(area.Actions, Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Admin)[AccessCatalog.ContentCompany], Is.EqualTo(area.Actions));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.ContentCompany], Is.EqualTo(AccessAction.View));
        });
    }

    private static string[] OperationsOf(string controller) => MappedEndpoints.Of(Map, controller)
        .Select(x => x.Name!).ToArray();
}

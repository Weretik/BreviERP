using System.Text.Json;
using BuildingBlocks.Api.Authorization;
using Identity.Domain.Authorization;
using Crm.Api.DependencyInjection;
using UnitTests.TestSupport;
using Crm.Application.Features.SiteRequests.Shared;
using Crm.Api.Endpoints.SiteRequests.SubmitPublicSiteRequest;
using Identity.Domain.Authorization.Roles;

namespace UnitTests.Crm.SiteRequests;

/// <summary>Site request types, operations and policies match the generated contract docs/sdd/contracts/openapi.json (LEAD-002).</summary>
public sealed class SiteRequestContractTests
{
    [TestCase("getCrmSiteRequests", null)]
    [TestCase("changeCrmSiteRequest", "edit")]
    public void Staff_operations_need_the_page(string operation, string? action)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCrmEndpoints(), operation);
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Policies, Is.EqualTo(new[] { $"access:{AccessCatalog.SiteRequests}" }));
            Assert.That(endpoint.Action, Is.EqualTo(action));
            Assert.That(ApiContract.OperationIds, Does.Contain(operation));
        });
    }

    [Test]
    public void The_site_posts_anonymously_and_the_page_is_in_access_rights()
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCrmEndpoints(), "submitPublicSiteRequest");
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Anonymous, Is.True);
            Assert.That(ApiContract.OperationIds, Does.Contain("submitPublicSiteRequest"));
            Assert.That(AccessCatalog.Find(AccessCatalog.SiteRequests)!.Title, Is.EqualTo("Заявки з сайту"));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Manager)[AccessCatalog.SiteRequests],
                Is.EqualTo(AccessAction.View | AccessAction.Edit));
            Assert.That(AccessCatalog.DefaultsOf(RoleNames.Seo), Does.Not.ContainKey(AccessCatalog.SiteRequests));
        });
    }
}

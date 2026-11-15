using System.Text.Json;
using Crm.Api.DependencyInjection;
using UnitTests.TestSupport;
using Crm.Application.Features.Leads.Shared;

namespace UnitTests.Crm.Leads;

/// <summary>Lead types, operations and policies match the generated contract docs/sdd/contracts/openapi.json (LEAD-001).</summary>
public sealed class LeadContractTests
{
    [TestCase("Leads", "getCrmLeads", "access:leads", null, null)]
    [TestCase("Leads", "getCrmLeadsMyDay", "access:leads", null, null)]
    [TestCase("Leads", "getCrmLeadCallResults", "access:leads", null, null)]
    [TestCase("Leads", "getCrmLead", "access:leads", null, null)]
    [TestCase("Leads", "takeCrmLead", "access:leads", null, "edit")]
    [TestCase("Leads", "releaseCrmLead", "access:leads", null, "edit")]
    [TestCase("Leads", "recordCrmLeadCall", "access:leads", null, "edit")]
    [TestCase("LeadStats", "getCrmLeadStats", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "getCrmLeadStatsCallResults", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "createCrmLeadCallResult", "access:lead-stats", null, "edit")]
    [TestCase("LeadStats", "editCrmLeadCallResult", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "getCrmLeadCategories", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "editCrmLeadCategory", "access:lead-stats", null, null)]
    [TestCase("LeadStats", "syncCrmLeads", "access:lead-stats", "access:lead-stats:create", null)]
    public void Operation_exists_with_its_policy(string tag, string operation, string groupPolicy,
        string? endpointPolicy, string? action)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapCrmEndpoints(), operation);
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Tag, Is.EqualTo(tag));
            Assert.That(endpoint.Policies, Is.EqualTo(new[] { groupPolicy, endpointPolicy }.OfType<string>()));
            Assert.That(endpoint.Action, Is.EqualTo(action));
            Assert.That(ApiContract.OperationIds, Does.Contain(operation));
        });
    }
}

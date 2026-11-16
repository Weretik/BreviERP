using ActivityLog.Api.DependencyInjection;
using Identity.Domain.Authorization;
using UnitTests.TestSupport;

namespace UnitTests.ActivityLog;

/// <summary>«Журнал дій» operations are read-only, need the activity-log page and are in the contract (ACT-001 SC-10).</summary>
public sealed class ActivityLogContractTests
{
    [TestCase("getActivityEntries")]
    [TestCase("getActivityFilters")]
    public void Operation_needs_the_activity_log_page(string operation)
    {
        var endpoint = MappedEndpoints.Named(app => app.MapActivityLogEndpoints(), operation);
        Assert.Multiple(() =>
        {
            Assert.That(endpoint.Method, Is.EqualTo("GET"));
            Assert.That(endpoint.Policies, Is.EqualTo(new[] { "access:activity-log" }));
            Assert.That(ApiContract.OperationIds, Does.Contain(operation));
        });
    }

    [Test]
    public void The_page_is_view_only_and_admin_by_default()
    {
        var area = AccessCatalog.Find(AccessCatalog.ActivityLog)!;
        Assert.That(area.Group, Is.EqualTo("Адміністрування"));
        Assert.That(area.Actions, Is.EqualTo(AccessAction.View));
        Assert.That(area.Defaults.Keys, Is.EqualTo(new[] { "Admin" }));
        Assert.That(MappedEndpoints.Of(app => app.MapActivityLogEndpoints()).Select(x => x.Method).Distinct(),
            Is.EqualTo(new[] { "GET" }));
    }
}

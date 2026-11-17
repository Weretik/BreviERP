using System.Xml.Linq;
namespace ArchitectureTests.Modules;

/// <summary>
/// «Журнал дій» (ACT-001, ADR-0016) knows no other module: they write to it through ActivityLog.Contracts, and Identity
/// gives it staff names through the port it declares there (ADR-0019). It owns schema activity_log with its own migration
/// history.
/// </summary>
public sealed class ActivityLogDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Crm", "Accounting", "Media", "Purchasing", "Warehouse", "Reminders", "Production", "Ai", "Content", "Scheduling", "Identity"];

    [Test]
    public void ActivityLog_references_no_other_module()
    {
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api", "Contracts" })
        {
            var references = XDocument.Load(Project(layer)).Descendants("ProjectReference")
                .Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => OtherModules.Any(m => x.Contains($"{m}."))), Is.False, layer);
            if (layer == "Contracts") Assert.That(references, Is.Empty);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
        }
    }

    [Test]
    public void ActivityLog_owns_its_schema_and_migration_history()
    {
        var infrastructure = Path.Combine(Root(), "src", "Modules", "ActivityLog", "ActivityLog.Infrastructure", "ActivityLog.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "ActivityLogDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "ActivityLogInfrastructureExtensions.cs"));
        Assert.That(context, Does.Contain("Schema = \"activity_log\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", ActivityLogDbContext.Schema)"));
    }

    private static string Project(string layer) =>
        Path.Combine(Root(), "src", "Modules", "ActivityLog", $"ActivityLog.{layer}", $"ActivityLog.{layer}", $"ActivityLog.{layer}.csproj");

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

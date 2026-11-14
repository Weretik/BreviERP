using System.Xml.Linq;
namespace ArchitectureTests.Modules;

/// <summary>«Фонові задачі» (SCH-001) depend on no other module: the job runners live in Host.Seed.</summary>
public sealed class SchedulingDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Media", "Purchasing", "Warehouse", "Reminders", "Production", "Ai", "Content"];

    [Test]
    public void SchedulingLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Scheduling"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root!.FullName, "src", "Modules", "Scheduling", $"Scheduling.{layer}", $"Scheduling.{layer}", $"Scheduling.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Scheduling.Api")), Is.False);
        }
    }

    [Test]
    public void SchedulingDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Scheduling"))) root = root.Parent;
        var infrastructure = Path.Combine(root!.FullName, "src", "Modules", "Scheduling", "Scheduling.Infrastructure", "Scheduling.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "SchedulingDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "SchedulingInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "SchedulingDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"scheduling\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", SchedulingDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", SchedulingDbContext.Schema)"));
    }
}

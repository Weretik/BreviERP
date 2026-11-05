using System.Xml.Linq;
namespace ArchitectureTests;

public sealed class PurchasingDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Reminders", "Media", "Production"];

    [Test]
    public void PurchasingLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = Root();
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root.FullName, "src", "Modules", "Purchasing", $"Purchasing.{layer}", $"Purchasing.{layer}", $"Purchasing.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Purchasing.Api")), Is.False);
            if (layer == "Api") Assert.That(references.Any(x => x.Contains("Purchasing.Infrastructure")), Is.False);
        }
    }

    [Test]
    public void PurchasingDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var infrastructure = Path.Combine(Root().FullName, "src", "Modules", "Purchasing", "Purchasing.Infrastructure", "Purchasing.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "PurchasingDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "PurchasingInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "PurchasingDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"purchasing\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", PurchasingDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", PurchasingDbContext.Schema)"));
    }

    private static DirectoryInfo Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Purchasing"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!;
    }
}

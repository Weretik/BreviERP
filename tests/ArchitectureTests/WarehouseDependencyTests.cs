using System.Xml.Linq;
namespace ArchitectureTests;

public sealed class WarehouseDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Reminders", "Media", "Production", "Ai", "Purchasing", "Content"];

    [Test]
    public void WarehouseLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = Root();
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root.FullName, "src", "Modules", "Warehouse", $"Warehouse.{layer}", $"Warehouse.{layer}", $"Warehouse.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Where(x => !x.EndsWith(".Contracts.csproj")).Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Warehouse.Api")), Is.False);
        }
    }

    [Test]
    public void WarehouseDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var infrastructure = Path.Combine(Root().FullName, "src", "Modules", "Warehouse", "Warehouse.Infrastructure", "Warehouse.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "WarehouseDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "WarehouseInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "WarehouseDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"warehouse\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", WarehouseDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", WarehouseDbContext.Schema)"));
    }

    private static DirectoryInfo Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Warehouse"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!;
    }
}

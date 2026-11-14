using System.Xml.Linq;
namespace ArchitectureTests.Modules.Office;

public sealed class AccountingDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Purchasing", "Reminders", "Media", "Production", "Ai", "Warehouse"];

    [Test]
    public void AccountingLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = Root();
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root.FullName, "src", "Modules", "Accounting", $"Accounting.{layer}", $"Accounting.{layer}", $"Accounting.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Where(x => !x.EndsWith(".Contracts.csproj")).Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Accounting.Api")), Is.False);
        }
    }

    [Test]
    public void AccountingDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var infrastructure = Path.Combine(Root().FullName, "src", "Modules", "Accounting", "Accounting.Infrastructure", "Accounting.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "AccountingDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "AccountingInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "AccountingDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"accounting\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", AccountingDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", AccountingDbContext.Schema)"));
    }

    private static DirectoryInfo Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Accounting"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!;
    }
}

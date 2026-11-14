using System.Xml.Linq;
namespace ArchitectureTests.Modules.Workshop;

public sealed class ProductionDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Reminders", "Media", "Purchasing", "Warehouse"];

    [Test]
    public void ProductionLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = RepositoryRoot();
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root, "src", "Modules", "Production", $"Production.{layer}", $"Production.{layer}", $"Production.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Where(x => !x.EndsWith(".Contracts.csproj")).Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Production.Api")), Is.False);
        }
    }

    [Test]
    public void ProductionDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var infrastructure = Path.Combine(RepositoryRoot(), "src", "Modules", "Production", "Production.Infrastructure", "Production.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "ProductionDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "ProductionInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "ProductionDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"production\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", ProductionDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", ProductionDbContext.Schema)"));
    }

    [Test]
    public void OtherModulesDoNotReferenceProduction()
    {
        var modules = Path.Combine(RepositoryRoot(), "src", "Modules");
        var references = OtherModules
            .SelectMany(module => Directory.GetFiles(Path.Combine(modules, module), "*.csproj", SearchOption.AllDirectories))
            .SelectMany(project => XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value));
        Assert.That(references.Any(x => x.Contains("Production.") && !x.EndsWith("Production.Contracts.csproj")), Is.False, "other modules use Production only through Production.Contracts");
    }

    private static string RepositoryRoot()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Production"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

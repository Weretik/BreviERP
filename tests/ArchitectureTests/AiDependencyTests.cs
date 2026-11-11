using System.Xml.Linq;
namespace ArchitectureTests;

public sealed class AiDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Reminders", "Media", "Production", "Purchasing", "Warehouse"];

    [Test]
    public void AiLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = Root();
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root.FullName, "src", "Modules", "Ai", $"Ai.{layer}", $"Ai.{layer}", $"Ai.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Ai.Api")), Is.False);
            if (layer == "Api") Assert.That(references.Any(x => x.Contains("Ai.Infrastructure")), Is.False);
        }
    }

    [Test]
    public void AiDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var infrastructure = Path.Combine(Root().FullName, "src", "Modules", "Ai", "Ai.Infrastructure", "Ai.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "AiDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "AiInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "AiDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"ai\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", AiDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", AiDbContext.Schema)"));
    }

    private static DirectoryInfo Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Ai"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!;
    }
}

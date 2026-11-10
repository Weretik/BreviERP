using System.Xml.Linq;
namespace ArchitectureTests;

public sealed class ContentDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Reminders", "Media", "Production", "Ai", "Purchasing"];

    [Test]
    public void ContentLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = Root();
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root.FullName, "src", "Modules", "Content", $"Content.{layer}", $"Content.{layer}", $"Content.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Content.Api")), Is.False);
            if (layer == "Api") Assert.That(references.Any(x => x.Contains("Content.Infrastructure")), Is.False);
        }
    }

    [Test]
    public void ContentDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var infrastructure = Path.Combine(Root().FullName, "src", "Modules", "Content", "Content.Infrastructure", "Content.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "ContentDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "ContentInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "ContentDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"content\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", ContentDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", ContentDbContext.Schema)"));
    }

    private static DirectoryInfo Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Content"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!;
    }
}

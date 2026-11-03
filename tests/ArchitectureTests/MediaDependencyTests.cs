using System.Xml.Linq;
namespace ArchitectureTests;

public sealed class MediaDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Reminders"];

    [Test]
    public void MediaLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = RepositoryRoot();
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root, "src", "Modules", "Media", $"Media.{layer}", $"Media.{layer}", $"Media.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Media.Api")), Is.False);
            if (layer == "Api") Assert.That(references.Any(x => x.Contains("Media.Infrastructure")), Is.False);
        }
    }

    [Test]
    public void MediaDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var infrastructure = Path.Combine(RepositoryRoot(), "src", "Modules", "Media", "Media.Infrastructure", "Media.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "MediaDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "MediaInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "MediaDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"media\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", MediaDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", MediaDbContext.Schema)"));
    }

    [Test]
    public void CatalogDoesNotReferenceMediaModule()
    {
        var catalog = Path.Combine(RepositoryRoot(), "src", "Modules", "Catalog");
        var references = Directory.GetFiles(catalog, "*.csproj", SearchOption.AllDirectories)
            .SelectMany(project => XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value));
        Assert.That(references.Any(x => x.Contains("Media.")), Is.False, "Catalog reads photos only through IProductMediaReader");
    }

    private static string RepositoryRoot()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Media"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

using System.Xml.Linq;
namespace ArchitectureTests.Modules.Office;

public sealed class RemindersDependencyTests
{
    private static readonly string[] OtherModules = ["Catalog", "Reference", "Identity", "Crm", "Accounting", "Media", "Purchasing", "Warehouse"];

    [Test]
    public void RemindersLayersReferenceOnlyBuildingBlocksAndLowerLayers()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Reminders"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root!.FullName, "src", "Modules", "Reminders", $"Reminders.{layer}", $"Reminders.{layer}", $"Reminders.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => OtherModules.Any(x.Contains)), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Reminders.Api")), Is.False);
        }
    }

    [Test]
    public void RemindersDbContextOwnsItsSchemaAndMigrationHistory()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Reminders"))) root = root.Parent;
        var infrastructure = Path.Combine(root!.FullName, "src", "Modules", "Reminders", "Reminders.Infrastructure", "Reminders.Infrastructure");
        var context = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "RemindersDbContext.cs"));
        var registration = File.ReadAllText(Path.Combine(infrastructure, "DependencyInjection", "RemindersInfrastructureExtensions.cs"));
        var factory = File.ReadAllText(Path.Combine(infrastructure, "Persistence", "RemindersDbContextFactory.cs"));
        Assert.That(context, Does.Contain("Schema = \"reminders\"").And.Contain("HasDefaultSchema(Schema)"));
        Assert.That(registration, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", RemindersDbContext.Schema)"));
        Assert.That(factory, Does.Contain("MigrationsHistoryTable(\"__EFMigrationsHistory\", RemindersDbContext.Schema)"));
    }
}

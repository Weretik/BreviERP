using System.Xml.Linq;
namespace ArchitectureTests;

public sealed class CrmDependencyTests
{
    [Test]
    public void CrmLayersDoNotReferenceOtherModulePersistenceOrUpperLayers()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !Directory.Exists(Path.Combine(root.FullName, "src", "Modules", "Crm"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        foreach (var layer in new[] { "Domain", "Application", "Infrastructure", "Api" })
        {
            var project = Path.Combine(root!.FullName, "src", "Modules", "Crm", $"Crm.{layer}", $"Crm.{layer}", $"Crm.{layer}.csproj");
            var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
            Assert.That(references.Any(x => x.Contains("Catalog") || x.Contains("Reference") || x.Contains("Identity")), Is.False, layer);
            if (layer == "Domain") Assert.That(references.All(x => x.Contains("BuildingBlocks.Domain")), Is.True);
            if (layer == "Application") Assert.That(references.Any(x => x.Contains("Infrastructure") || x.Contains("Crm.Api")), Is.False);
        }
    }
}

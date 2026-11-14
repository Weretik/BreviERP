using System.Xml.Linq;
namespace ArchitectureTests;

public sealed class ContractsDependencyTests
{
    private static readonly string[] Modules = ["Accounting", "Ai", "Catalog", "Content", "Crm", "Identity", "Media", "Production", "Purchasing", "Reference", "Reminders", "Warehouse"];

    [TestCaseSource(nameof(Modules))]
    public void Every_module_has_a_contracts_project_without_module_references(string module)
    {
        var project = Path.Combine(Root(), "src", "Modules", module, $"{module}.Contracts", $"{module}.Contracts", $"{module}.Contracts.csproj");
        Assert.That(File.Exists(project), Is.True, $"{module}.Contracts is required by the module rules");
        var references = XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value).ToArray();
        Assert.That(references, Is.Empty, "Contracts reference no module project");
    }

    [TestCaseSource(nameof(Modules))]
    public void Other_modules_are_referenced_only_through_their_contracts(string module)
    {
        var modulePath = Path.Combine(Root(), "src", "Modules", module);
        var others = Modules.Where(x => x != module).ToArray();
        var offending = Directory.GetFiles(modulePath, "*.csproj", SearchOption.AllDirectories)
            .SelectMany(project => XDocument.Load(project).Descendants("ProjectReference").Select(x => x.Attribute("Include")!.Value.Replace('\\', '/')))
            .Where(reference => others.Any(other => reference.Contains($"/{other}.")) && !reference.EndsWith(".Contracts.csproj"))
            .ToArray();
        Assert.That(offending, Is.Empty);
    }

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

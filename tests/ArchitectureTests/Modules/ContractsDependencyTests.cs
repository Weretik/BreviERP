using System.Xml.Linq;
namespace ArchitectureTests.Modules;

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

    /// <summary>
    /// The module graph has no cycle (ADR-0019): when two modules need each other, the upstream module declares a port
    /// in its own Contracts and the downstream module implements it, so the reference goes one way only.
    /// </summary>
    [Test]
    public void Modules_do_not_depend_on_each_other_in_a_cycle()
    {
        var modulesPath = Path.Combine(Root(), "src", "Modules");
        var all = Directory.GetDirectories(modulesPath).Select(Path.GetFileName).OfType<string>().ToArray();
        var graph = all.ToDictionary(module => module, module => Directory
            .GetFiles(Path.Combine(modulesPath, module), "*.csproj", SearchOption.AllDirectories)
            .SelectMany(project => XDocument.Load(project).Descendants("ProjectReference")
                .Select(x => x.Attribute("Include")!.Value.Replace('\\', '/')))
            .SelectMany(reference => all.Where(other => other != module && reference.Contains($"/{other}/")))
            .Distinct()
            .Order()
            .ToArray());

        var cycles = new List<string>();
        var done = new HashSet<string>();
        foreach (var module in all.Order())
            Visit(module, []);

        Assert.That(cycles, Is.Empty, "turn one direction into a port the upstream module declares (ADR-0019)");

        void Visit(string module, List<string> path)
        {
            if (path.Contains(module))
            {
                cycles.Add(string.Join(" → ", path.Skip(path.IndexOf(module)).Append(module)));
                return;
            }
            if (!done.Add(module)) return;
            path.Add(module);
            foreach (var next in graph[module]) Visit(next, path);
            path.RemoveAt(path.Count - 1);
        }
    }

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

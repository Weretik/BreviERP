using System.Xml.Linq;
namespace ArchitectureTests;

/// <summary>Solution root: shared settings in Directory.Build.props, every package version only in Directory.Packages.props.</summary>
public sealed class SolutionRootTests
{
    [Test]
    public void Projects_take_package_versions_and_shared_settings_from_the_solution_root()
    {
        var root = Root();
        var offending = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .SelectMany(f =>
            {
                var project = XDocument.Load(f);
                var name = Path.GetRelativePath(root, f);
                return project.Descendants("PackageReference").Where(x => x.Attribute("Version") is not null || x.Element("Version") is not null)
                    .Select(x => $"{name}: {x.Attribute("Include")?.Value} has its own version")
                    .Concat(project.Descendants().Where(x => x.Name.LocalName is "TargetFramework" or "Nullable" or "ImplicitUsings")
                        .Select(x => $"{name}: {x.Name.LocalName} belongs in Directory.Build.props"));
            })
            .ToArray();
        Assert.That(offending, Is.Empty);
        Assert.That(File.Exists(Path.Combine(root, "Directory.Packages.props")), Is.True);
        Assert.That(File.Exists(Path.Combine(root, "global.json")), Is.True);
    }

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

using System.Text.RegularExpressions;

namespace ArchitectureTests.Folders;

/// <summary>
/// Every project lives on disk under <c>src/</c> or <c>tests/</c>, and the solution shows it under the solution folder of
/// the same name: Rider and Visual Studio show the solution folders, so a project folder left at the solution root
/// (as «Jobs» was) looks like a separate layer outside <c>src</c>.
/// </summary>
public sealed partial class SolutionFoldersTests
{
    [Test]
    public void Projects_live_under_src_or_tests()
    {
        Assert.That(Projects().Count(), Is.GreaterThan(10), "the solution file was not parsed");
        var offending = Projects().Where(p => !(p.Path.StartsWith("src/") || p.Path.StartsWith("tests/")))
            .Select(p => p.Path)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Solution_folders_match_the_disk_top_folder()
    {
        var solution = File.ReadAllText(Path.Combine(Root(), "BreviERP.sln"));
        var names = ProjectLine().Matches(solution).ToDictionary(m => m.Groups["id"].Value, m => m.Groups["name"].Value);
        var parents = NestedLine().Matches(solution).ToDictionary(m => m.Groups["child"].Value, m => m.Groups["parent"].Value);

        var offending = Projects()
            .Where(p => TopSolutionFolder(p.Id, parents, names) != p.Path.Split('/')[0])
            .Select(p => $"{p.Path} is under solution folder «{TopSolutionFolder(p.Id, parents, names)}»")
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    private static string TopSolutionFolder(string id, Dictionary<string, string> parents, Dictionary<string, string> names)
    {
        var current = id;
        while (parents.TryGetValue(current, out var parent)) current = parent;
        return current == id ? "(solution root)" : names[current];
    }

    private static IEnumerable<(string Id, string Path)> Projects() =>
        ProjectLine().Matches(File.ReadAllText(Path.Combine(Root(), "BreviERP.sln")))
            .Where(m => m.Groups["path"].Value.EndsWith(".csproj", StringComparison.Ordinal))
            .Select(m => (m.Groups["id"].Value, m.Groups["path"].Value.Replace('\\', '/')));

    [GeneratedRegex("Project\\(\"\\{[^}]+\\}\"\\) = \"(?<name>[^\"]+)\", \"(?<path>[^\"]+)\", \"\\{(?<id>[^}]+)\\}\"")]
    private static partial Regex ProjectLine();

    [GeneratedRegex(@"\{(?<child>[0-9A-Fa-f-]+)\} = \{(?<parent>[0-9A-Fa-f-]+)\}")]
    private static partial Regex NestedLine();

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

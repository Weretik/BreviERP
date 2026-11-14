using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ArchitectureTests.Modules;

public sealed class ContainerRestoreCoverageTests
{
    [TestCase("Host.Api")]
    [TestCase("Host.Seed")]
    public void Every_transitive_project_is_copied_before_container_restore(string host)
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null, "Repository root must be available to check the container inputs.");
        var root = directory!.FullName;
        var project = Path.Combine(root, "src", "Bootstrapper", host, host + ".csproj");
        var dockerfile = File.ReadAllText(Path.Combine(Path.GetDirectoryName(project)!, "Dockerfile"));
        var restoreIndex = dockerfile.IndexOf("RUN dotnet restore", StringComparison.Ordinal);
        Assert.That(restoreIndex, Is.GreaterThan(0));
        var beforeRestore = dockerfile[..restoreIndex];
        var copied = new HashSet<string>(StringComparer.Ordinal);
        foreach (Match match in Regex.Matches(beforeRestore, @"^COPY\s+(\[[^\r\n]+\])", RegexOptions.Multiline))
        {
            var arguments = JsonSerializer.Deserialize<string[]>(match.Groups[1].Value)!;
            foreach (var source in arguments[..^1]) copied.Add(Path.GetFullPath(Path.Combine(root, source)));
        }

        var visited = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(project);
        while (pending.TryPop(out var candidate))
        {
            var current = Path.GetFullPath(candidate);
            if (!visited.Add(current)) continue;
            foreach (var reference in XDocument.Load(current).Descendants("ProjectReference"))
            {
                var include = reference.Attribute("Include")!.Value.Replace('\\', Path.DirectorySeparatorChar);
                pending.Push(Path.Combine(Path.GetDirectoryName(current)!, include));
            }
        }

        foreach (var shared in new[] { "global.json", "Directory.Build.props", "Directory.Packages.props" })
            visited.Add(Path.GetFullPath(Path.Combine(root, shared)));

        var missing = visited.Except(copied).Select(path => Path.GetRelativePath(root, path)).Order().ToArray();
        Assert.That(missing, Is.Empty,
            "Projects copied only after restore have no NuGet assets during publish --no-restore.");
    }
}

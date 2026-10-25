using System.Xml.Linq;

namespace ArchitectureTests;

public sealed class JobDependencyTests
{
    [Test]
    public void Runtime_modules_and_api_do_not_depend_on_operational_jobs()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        var root = directory!.FullName;
        var jobs = Path.Combine(root, "src", "Jobs") + Path.DirectorySeparatorChar;
        var projects = Directory.GetFiles(Path.Combine(root, "src", "Modules"), "*.csproj", SearchOption.AllDirectories)
            .Append(Path.Combine(root, "src", "Bootstrapper", "Host.Api", "Host.Api.csproj"));
        var violations = new List<string>();
        foreach (var project in projects)
        {
            foreach (var reference in XDocument.Load(project).Descendants("ProjectReference"))
            {
                var include = reference.Attribute("Include")?.Value;
                if (include is null)
                    continue;
                var portableInclude = include.Replace('\\', Path.DirectorySeparatorChar);
                var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(project)!, portableInclude));
                if (resolved.StartsWith(jobs, StringComparison.OrdinalIgnoreCase))
                    violations.Add($"{Path.GetRelativePath(root, project)} -> {include}");
            }
        }
        Assert.That(violations, Is.Empty);
    }
}

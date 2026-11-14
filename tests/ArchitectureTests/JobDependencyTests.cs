using System.Text.RegularExpressions;
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

    /// <summary>
    /// A Host.Seed job only parses its arguments and runs a module use case (through ISender) or a src/Jobs
    /// project: it reaches no module Infrastructure, DbContext or module service directly.
    /// </summary>
    [Test]
    public void Seed_host_jobs_are_thin_runners()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        var jobs = Path.Combine(directory!.FullName, "src", "Bootstrapper", "Host.Seed", "Jobs");
        var allowedServices = new[] { "ISender", "ILoggerFactory" };
        var violations = new List<string>();
        foreach (var file in Directory.GetFiles(jobs, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            var name = Path.GetRelativePath(jobs, file).Replace('\\', '/');
            foreach (Match match in Regex.Matches(text, @"^using\s+(\w+)\.Infrastructure\b", RegexOptions.Multiline))
                if (match.Groups[1].Value != "BuildingBlocks")
                    violations.Add($"{name}: using {match.Groups[1].Value}.Infrastructure");
            foreach (Match match in Regex.Matches(text, @"GetRequiredService<(\w+)>"))
                if (!allowedServices.Contains(match.Groups[1].Value))
                    violations.Add($"{name}: resolves {match.Groups[1].Value}");
            if (Regex.IsMatch(text, @"\bDbContext\b|\bCreateDbContext"))
                violations.Add($"{name}: uses a DbContext");
        }
        Assert.That(violations, Is.Empty);
    }
}

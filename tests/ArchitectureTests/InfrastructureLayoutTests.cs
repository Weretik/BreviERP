namespace ArchitectureTests;

/// <summary>
/// Infrastructure projects group persistence by aggregate and outside systems by system (module rules «Folders»):
/// <c>Persistence/</c> (DbContext, migrations, <c>&lt;Aggregate&gt;/</c>, <c>Seeding/</c>), <c>Integrations/&lt;System&gt;/</c>,
/// <c>Adapters/&lt;Provider&gt;/</c>, <c>Contracts/&lt;Feature&gt;/</c>, <c>DependencyInjection/</c>.
/// </summary>
public sealed class InfrastructureLayoutTests
{
    private static readonly string[] TopFolders = ["Persistence", "Integrations", "Adapters", "Contracts", "DependencyInjection"];

    private static readonly string[] KindFolders =
        ["Configurations", "Congigurations", "Configuration", "DataBase", "Repositories", "Seeders", "Entities", "Services", "Records", "Reading", "Writing"];

    [Test]
    public void Infrastructure_projects_have_only_the_rule_folders_at_the_root()
    {
        var offending = InfrastructureProjects()
            .SelectMany(project => Folders(project).Where(dir => !TopFolders.Contains(Path.GetFileName(dir))))
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Infrastructure_projects_have_no_folders_named_after_kinds_of_classes()
    {
        var offending = InfrastructureProjects()
            .SelectMany(project => Directory.GetDirectories(project, "*", SearchOption.AllDirectories))
            .Where(NotBuildOutput)
            .Where(dir => KindFolders.Contains(Path.GetFileName(dir)))
            .Where(dir => Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories).Any())
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Entity_configurations_live_in_persistence()
    {
        var offending = InfrastructureProjects()
            .SelectMany(project => Directory.EnumerateFiles(project, "*.cs", SearchOption.AllDirectories)
                .Where(NotBuildOutput)
                .Where(file => File.ReadAllText(file).Contains("IEntityTypeConfiguration<", StringComparison.Ordinal))
                .Where(file => !Path.GetRelativePath(project, file).StartsWith("Persistence" + Path.DirectorySeparatorChar, StringComparison.Ordinal)))
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    private static IEnumerable<string> InfrastructureProjects() =>
        Directory.GetFiles(Path.Combine(Root(), "src", "Modules"), "*.Infrastructure.csproj", SearchOption.AllDirectories)
            .Select(project => Path.GetDirectoryName(project)!);

    private static IEnumerable<string> Folders(string dir) =>
        Directory.GetDirectories(dir).Where(NotBuildOutput).Where(x => Directory.EnumerateFiles(x, "*.cs", SearchOption.AllDirectories).Any());

    private static bool NotBuildOutput(string path) =>
        !path.Contains($"{Path.DirectorySeparatorChar}bin") && !path.Contains($"{Path.DirectorySeparatorChar}obj");

    private static string Relative(string path) => Path.GetRelativePath(Root(), path);

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

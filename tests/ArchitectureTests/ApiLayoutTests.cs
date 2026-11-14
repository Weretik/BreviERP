namespace ArchitectureTests;

/// <summary>
/// Api projects group HTTP code by feature, the same names as in Application (module rules «Folders»): a feature folder holds
/// its controller (later its endpoints) with the requests, responses and mappers next to it; <c>Access/</c> holds policy names
/// and <c>DependencyInjection/</c> the registration. No folder is named after a kind of class.
/// </summary>
public sealed class ApiLayoutTests
{
    private static readonly string[] KindFolders =
        ["Controllers", "Contracts", "Requests", "Responses", "Mapping", "Mappers", "Models", "Dtos", "DTOs", "Options", "Results", "Filters", "Services", "Helpers", "Extensions"];

    [Test]
    public void Api_projects_have_no_folders_named_after_kinds_of_classes()
    {
        var offending = ApiProjects()
            .SelectMany(project => Directory.GetDirectories(project, "*", SearchOption.AllDirectories))
            .Where(NotBuildOutput)
            .Where(dir => KindFolders.Contains(Path.GetFileName(dir)))
            .Where(dir => Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories).Any())
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Api_projects_keep_code_in_folders()
    {
        var offending = ApiProjects()
            .SelectMany(project => Directory.GetFiles(project, "*.cs"))
            .Where(file => !Path.GetFileName(file).EndsWith("ApiAssemblyMarker.cs", StringComparison.Ordinal) && Path.GetFileName(file) != "GlobalUsing.cs")
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    private static IEnumerable<string> ApiProjects() =>
        Directory.GetFiles(Path.Combine(Root(), "src", "Modules"), "*.Api.csproj", SearchOption.AllDirectories)
            .Select(project => Path.GetDirectoryName(project)!);

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

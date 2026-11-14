using System.Text.RegularExpressions;

namespace ArchitectureTests.Layers;

/// <summary>
/// Application projects hold one use case per folder: <c>Features/&lt;Feature&gt;/{Commands,Queries}/&lt;UseCase&gt;/</c>
/// with <c>&lt;UseCase&gt;Handler</c> on top (named subfolders only past six files); types shared by use cases sit in the feature's <c>Shared/</c>, ports in
/// <c>Abstractions/</c> (module rules «Folders»).
/// </summary>
public sealed partial class ApplicationLayoutTests
{
    private static readonly string[] TopFolders = ["Abstractions", "Features", "DependencyInjection"];
    private static readonly string[] FeatureFolders = ["Commands", "Queries", "EventHandlers", "Shared"];

    /// <summary>Identity keeps its access policies and claims beside the use cases: they are read by every module's Api.</summary>
    private static readonly string[] AllowedExtraFolders = ["Identity.Application/Security"];

    [Test]
    public void Application_projects_have_only_abstractions_features_and_composition_at_the_root()
    {
        var offending = ApplicationProjects()
            .SelectMany(app => Folders(app)
                .Where(dir => !TopFolders.Contains(Path.GetFileName(dir)))
                .Where(dir => !AllowedExtraFolders.Any(x => dir.Replace('\\', '/').EndsWith(x, StringComparison.Ordinal)))
                .Select(Relative))
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Features_split_into_commands_queries_and_shared()
    {
        var offending = ApplicationProjects()
            .Select(app => Path.Combine(app, "Features"))
            .Where(Directory.Exists)
            .SelectMany(Folders)
            .SelectMany(feature => FeatureFolders.Contains(Path.GetFileName(feature))
                ? [feature]
                : Folders(feature).Where(dir => !FeatureFolders.Contains(Path.GetFileName(dir)))
                    .Concat(Directory.GetFiles(feature, "*.cs")))
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Every_handler_lives_in_its_own_use_case_folder_and_is_named_after_it()
    {
        var offending = ApplicationProjects()
            .SelectMany(app => Directory.EnumerateFiles(app, "*.cs", SearchOption.AllDirectories).Where(NotBuildOutput))
            .Where(file => HandlerPattern().IsMatch(File.ReadAllText(file)))
            .Where(file =>
            {
                var useCase = Path.GetDirectoryName(file)!;
                var kind = Path.GetFileName(Path.GetDirectoryName(useCase));
                return kind is not ("Commands" or "Queries")
                       || Path.GetFileNameWithoutExtension(file) != Path.GetFileName(useCase) + "Handler";
            })
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    /// <summary>
    /// A use case stays flat while it fits six files; past that its request, handler and validator stay on top and the
    /// rest goes into named subfolders (<c>Specs/</c>, <c>Rows/</c>, <c>Results/</c> or a sub-concept). A small use case with
    /// subfolders is a needless split.
    /// </summary>
    [Test]
    public void Use_case_folders_split_only_past_six_files()
    {
        var offending = ApplicationProjects()
            .Select(app => Path.Combine(app, "Features"))
            .Where(Directory.Exists)
            .SelectMany(Folders)
            .SelectMany(feature => Folders(feature).Where(dir => Path.GetFileName(dir) is "Commands" or "Queries"))
            .SelectMany(Folders)
            .Where(useCase => Folders(useCase).Any()
                              && Directory.EnumerateFiles(useCase, "*.cs", SearchOption.AllDirectories).Count(NotBuildOutput) <= 6)
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [GeneratedRegex(@"\bI(Command|Query)Handler<")]
    private static partial Regex HandlerPattern();

    private static IEnumerable<string> ApplicationProjects() =>
        Directory.GetFiles(Path.Combine(Root(), "src", "Modules"), "*.Application.csproj", SearchOption.AllDirectories)
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

namespace ArchitectureTests.Folders;

/// <summary>
/// The folder axiom (module rules «Folder size»): a code folder holds at most six <c>.cs</c> files; past that it splits by
/// meaning into named subfolders. No empty folders and no wrapper folders (no files and a single subfolder), except the
/// project container (<c>Catalog.Api/Catalog.Api</c>) and the layout levels the module rules require. Generated code
/// (EF <c>Migrations/</c>) and build output are exempt. Checks <c>src/</c> and <c>tests/</c>.
/// </summary>
public sealed class FolderSizeTests
{
    private const int MaxFiles = 6;

    private static readonly string[] Exempt = ["bin", "obj", "Migrations", "TestResults", "node_modules"];

    /// <summary>Folders that exist because the layout says so, even when they hold one entry.</summary>
    private static readonly string[] LayoutLevels =
        ["Features", "Commands", "Queries", "EventHandlers", "Endpoints", "Contracts", "Adapters", "Integrations", "Seeding"];

    [Test]
    public void Code_folders_hold_at_most_six_files()
    {
        var offending = CodeFolders()
            .Where(dir => Directory.GetFiles(dir, "*.cs").Length > MaxFiles)
            .Select(dir => $"{Relative(dir)} ({Directory.GetFiles(dir, "*.cs").Length})")
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Code_folders_are_not_empty()
    {
        var offending = CodeFolders()
            .Where(dir => Directory.GetFiles(dir).Length == 0 && SubFolders(dir).Length == 0)
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Code_folders_are_not_wrappers()
    {
        var offending = CodeFolders()
            .Where(dir => Directory.GetFiles(dir).Length == 0 && SubFolders(dir) is [var only]
                          && Path.GetFileName(only) != Path.GetFileName(dir)
                          && !LayoutLevels.Contains(Path.GetFileName(dir))
                          && Path.GetFileName(Path.GetDirectoryName(dir)) != "Features")
            .Select(Relative)
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    private static IEnumerable<string> CodeFolders() =>
        new[] { "src", "tests" }
            .Select(top => Path.Combine(Root(), top))
            .SelectMany(top => Directory.EnumerateDirectories(top, "*", SearchOption.AllDirectories))
            .Where(dir => !Path.GetRelativePath(Root(), dir).Split(Path.DirectorySeparatorChar).Any(Exempt.Contains));

    private static string[] SubFolders(string dir) =>
        Directory.GetDirectories(dir).Where(x => !Exempt.Contains(Path.GetFileName(x))).ToArray();

    private static string Relative(string path) => Path.GetRelativePath(Root(), path);

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

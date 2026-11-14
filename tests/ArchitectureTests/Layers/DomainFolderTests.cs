namespace ArchitectureTests.Layers;

/// <summary>
/// Domain folders follow aggregates (module rules «Folders»). Inside an aggregate folder a large aggregate may split its
/// value objects, enums and errors into <c>ValueObjects/</c>, <c>Enums/</c> and <c>Errors/</c>; the project root never
/// has such type folders, and <c>Entities/</c>, <c>Constants/</c>, <c>Exceptions/</c>, <c>Interfaces/</c> are not used.
/// </summary>
public sealed class DomainFolderTests
{
    private static readonly string[] TypeFolders = ["Entities", "Constants", "Exceptions", "Interfaces"];
    private static readonly string[] AggregateTypeFolders = ["ValueObjects", "Enums", "Errors"];

    [Test]
    public void Domain_projects_have_no_folders_named_after_kinds_of_classes()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);

        var offending = Directory.GetDirectories(Path.Combine(root!.FullName, "src", "Modules"), "*.Domain", SearchOption.AllDirectories)
            .SelectMany(domain => Directory.GetDirectories(domain, "*", SearchOption.AllDirectories)
                .Where(dir => !dir.Contains($"{Path.DirectorySeparatorChar}bin") && !dir.Contains($"{Path.DirectorySeparatorChar}obj"))
                .Where(dir => TypeFolders.Contains(Path.GetFileName(dir))
                              || (AggregateTypeFolders.Contains(Path.GetFileName(dir)) && Path.GetDirectoryName(dir) == domain))
                .Where(dir => Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories).Any())
                .Select(dir => Path.GetRelativePath(root.FullName, dir)))
            .Distinct()
            .ToArray();

        Assert.That(offending, Is.Empty);
    }
}

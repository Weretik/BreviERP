namespace ArchitectureTests;

/// <summary>Domain folders follow aggregates, never kinds of classes (module rules «Folders»).</summary>
public sealed class DomainFolderTests
{
    private static readonly string[] TypeFolders = ["Entities", "ValueObjects", "Enums", "Errors", "Constants", "Exceptions", "Interfaces"];

    [Test]
    public void Domain_projects_have_no_folders_named_after_kinds_of_classes()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);

        var offending = Directory.GetDirectories(Path.Combine(root!.FullName, "src", "Modules"), "*.Domain", SearchOption.AllDirectories)
            .SelectMany(domain => Directory.GetDirectories(domain, "*", SearchOption.AllDirectories)
                .Where(dir => !dir.Contains($"{Path.DirectorySeparatorChar}bin") && !dir.Contains($"{Path.DirectorySeparatorChar}obj"))
                .Where(dir => TypeFolders.Contains(Path.GetFileName(dir)))
                .Where(dir => Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories).Any())
                .Select(dir => Path.GetRelativePath(root.FullName, dir)))
            .Distinct()
            .ToArray();

        Assert.That(offending, Is.Empty);
    }
}

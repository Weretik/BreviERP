using System.Text.RegularExpressions;

namespace ArchitectureTests.Layers;

/// <summary>
/// Domain model rules (module rules «Domain model (DDD)»): a root carrying <c>IAggregateRoot</c> is created by a static
/// factory, never by a public constructor; a module's persistence port in Application exposes a writable <c>DbSet</c>
/// only for roots (children are read through an <c>IQueryable</c>), so a handler cannot write a child past its root; and
/// domain properties have no public setter.
/// </summary>
public sealed partial class AggregateRootTests
{
    private static readonly string Root = FindRoot();
    private static readonly IReadOnlyList<string> DomainFiles = SourceFiles("*.Domain");
    private static readonly IReadOnlySet<string> Roots = DomainFiles
        .SelectMany(file => ClassHeader().Matches(File.ReadAllText(file)))
        .Where(match => match.Groups["bases"].Value.Contains("IAggregateRoot", StringComparison.Ordinal))
        .Select(match => match.Groups["name"].Value)
        .ToHashSet(StringComparer.Ordinal);

    [Test]
    public void Every_business_module_marks_its_roots()
    {
        // Identity keeps users and roles in ASP.NET Identity tables and has no aggregate of its own.
        var withoutRoots = DomainFiles
            .GroupBy(file => Path.GetRelativePath(Path.Combine(Root, "src", "Modules"), file).Split(Path.DirectorySeparatorChar)[0])
            .Where(module => module.Key != "Identity" && !module.Any(file => ClassHeader().Matches(File.ReadAllText(file))
                .Any(match => match.Groups["bases"].Value.Contains("IAggregateRoot", StringComparison.Ordinal))))
            .Select(module => module.Key)
            .ToArray();

        Assert.That(withoutRoots, Is.Empty);
    }

    [Test]
    public void Roots_are_created_by_a_factory_not_a_public_constructor()
    {
        var offending = DomainFiles
            .SelectMany(file => Roots.Where(root =>
                    Regex.IsMatch(File.ReadAllText(file), $@"^\s+public {root}\(", RegexOptions.Multiline))
                .Select(root => $"{Path.GetRelativePath(Root, file)}: {root}"))
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Persistence_ports_expose_a_writable_DbSet_only_for_roots()
    {
        var offending = SourceFiles("*.Application")
            .Where(file => file.Contains($"{Path.DirectorySeparatorChar}Abstractions{Path.DirectorySeparatorChar}"))
            .SelectMany(file => DbSetType().Matches(File.ReadAllText(file))
                .Select(match => match.Groups["type"].Value)
                .Where(type => type != "OutboxMessage" && !Roots.Contains(type))
                .Select(type => $"{Path.GetRelativePath(Root, file)}: DbSet<{type}>"))
            .ToArray();

        Assert.That(offending, Is.Empty, "a child is read through IQueryable<T> and written only through its root");
    }

    [Test]
    public void Domain_properties_have_no_public_setter()
    {
        var offending = DomainFiles
            .SelectMany(file => File.ReadLines(file)
                .Where(line => PublicSetter().IsMatch(line))
                .Select(line => $"{Path.GetRelativePath(Root, file)}: {line.Trim()}"))
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    private static IReadOnlyList<string> SourceFiles(string projectPattern) =>
        Directory.GetDirectories(Path.Combine(Root, "src", "Modules"), projectPattern, SearchOption.AllDirectories)
            .Where(dir => File.Exists(Path.Combine(dir, Path.GetFileName(dir) + ".csproj")))
            .SelectMany(dir => Directory.EnumerateFiles(dir, "*.cs", SearchOption.AllDirectories))
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                           && !file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToArray();

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "BreviERP.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("BreviERP.sln not found.");
    }

    [GeneratedRegex(@"^public (?:sealed |abstract )?(?:partial )?class (?<name>\w+)(?<bases>[^\n{]*)", RegexOptions.Multiline)]
    private static partial Regex ClassHeader();

    [GeneratedRegex(@"DbSet<(?<type>\w+)>")]
    private static partial Regex DbSetType();

    [GeneratedRegex(@"^\s*public [^(]*\{\s*get;\s*set;")]
    private static partial Regex PublicSetter();
}

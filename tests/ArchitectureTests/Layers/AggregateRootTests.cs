using System.Text.RegularExpressions;

namespace ArchitectureTests.Layers;

/// <summary>
/// Domain model rules (module rules «Domain model (DDD)»): a root carrying <c>IAggregateRoot</c> is created by a static
/// factory, never by a public constructor; a module's persistence port in Application exposes a writable <c>DbSet</c>
/// only for roots (children are read through an <c>IQueryable</c>), so a handler cannot write a child past its root;
/// domain properties have no public setter; and a change history is never loaded with its root (the root's
/// <c>New*</c> navigation holds only the lines a command adds; the saved history is read by a query); and a root with
/// an <c>Id</c> has its own id type, never a bare number, text or GUID.
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

    [Test]
    public void A_change_history_is_never_loaded_with_its_root()
    {
        var offending = SourceFiles("*.Application").Concat(SourceFiles("*.Infrastructure"))
            .SelectMany(file => HistoryLoad().Matches(File.ReadAllText(file))
                .Select(match => $"{Path.GetRelativePath(Root, file)}: {match.Value}"))
            .ToArray();

        Assert.That(offending, Is.Empty, "read the saved history through the module's IQueryable, not the root's New* list");
    }

    [Test]
    public void Roots_have_a_typed_id()
    {
        var offending = DomainFiles
            .SelectMany(file => ClassBodies(File.ReadAllText(file))
                .Where(body => Roots.Contains(body.Name) && BareId().IsMatch(body.Text))
                .Select(body => $"{Path.GetRelativePath(Root, file)}: {body.Name}"))
            .ToArray();

        Assert.That(offending, Is.Empty, "give the root a readonly record struct <Root>Id mapped by AddTypedIds");
    }

    /// <summary>Each top-level class of a file with the text up to the next one.</summary>
    private static IEnumerable<(string Name, string Text)> ClassBodies(string text)
    {
        var headers = ClassHeader().Matches(text);
        for (var i = 0; i < headers.Count; i++)
        {
            var end = i + 1 < headers.Count ? headers[i + 1].Index : text.Length;
            yield return (headers[i].Groups["name"].Value, text[headers[i].Index..end]);
        }
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

    [GeneratedRegex(@"(?:Include|Collection)\(\s*\w+\s*=>\s*\w+\.New[A-Z]\w*")]
    private static partial Regex HistoryLoad();

    [GeneratedRegex(@"^\s+public (?:int|long|string|Guid) Id \{", RegexOptions.Multiline)]
    private static partial Regex BareId();

    [GeneratedRegex(@"DbSet<(?<type>\w+)>")]
    private static partial Regex DbSetType();

    [GeneratedRegex(@"^\s*public [^(]*\{\s*get;\s*set;")]
    private static partial Regex PublicSetter();
}

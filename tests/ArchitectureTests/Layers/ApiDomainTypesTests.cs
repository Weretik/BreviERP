using System.Text.RegularExpressions;

namespace ArchitectureTests.Layers;

/// <summary>
/// Api code holds no domain type (module rules «Handlers and endpoints»): requests, responses and controllers use the
/// Application's commands, queries and DTOs. Domain enums are the one exception: they are the plain value lists that the
/// OpenAPI contract shows by name. The files listed below still take a domain text type as
/// part of the request and wait for their own request types.
/// </summary>
public sealed partial class ApiDomainTypesTests
{
    private static readonly string[] Waiting =
    [
        "Content.Api/Endpoints/Articles/Shared/SaveContentArticleRequest.cs",
        "Content.Api/Endpoints/ProductPages/SaveContentProduct/SaveContentProductRequest.cs",
        "Content.Api/Endpoints/SiteCategories/Shared/SaveSiteCategoryRequest.cs",
    ];

    [Test]
    public void Api_code_uses_only_domain_enums()
    {
        var types = DomainTypes();
        var offending = Directory.GetFiles(Path.Combine(Root(), "src", "Modules"), "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains(".Api" + Path.DirectorySeparatorChar) && NotBuildOutput(f))
            .Where(f => !Waiting.Any(w => f.Replace('\\', '/').EndsWith(w, StringComparison.Ordinal)))
            .SelectMany(f =>
            {
                var text = File.ReadAllText(f);
                var body = CodeBody().Replace(text, "");
                return DomainUsing().Matches(text).Select(m => m.Groups[1].Value)
                    .SelectMany(ns => types.GetValueOrDefault(ns, []))
                    .Where(t => t.Kind != "enum" && Regex.IsMatch(body, $@"\b{t.Name}\b"))
                    .Select(t => $"{Relative(f)}: {t.Name}");
            })
            .ToArray();

        Assert.That(offending, Is.Empty);
    }

    private static Dictionary<string, List<(string Name, string Kind)>> DomainTypes()
    {
        var result = new Dictionary<string, List<(string Name, string Kind)>>();
        var files = Directory.GetFiles(Path.Combine(Root(), "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => f.Contains(".Domain" + Path.DirectorySeparatorChar) && NotBuildOutput(f));
        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var ns = Namespace().Match(text);
            if (!ns.Success) continue;
            var list = result.TryGetValue(ns.Groups[1].Value, out var found) ? found : result[ns.Groups[1].Value] = [];
            list.AddRange(TypeDeclaration().Matches(text).Select(m => (m.Groups[2].Value, m.Groups[1].Value)));
        }

        return result;
    }

    [GeneratedRegex(@"^using\s+([\w.]*\.Domain(?:\.[\w.]+)?)\s*;", RegexOptions.Multiline)]
    private static partial Regex DomainUsing();

    [GeneratedRegex(@"^\s*(?:using|namespace)\b.*$|\b\w+(?:\.\w+)*\.Api(?:\.\w+)+", RegexOptions.Multiline)]
    private static partial Regex CodeBody();

    [GeneratedRegex(@"^\s*namespace\s+([\w.]+)", RegexOptions.Multiline)]
    private static partial Regex Namespace();

    [GeneratedRegex(@"^\s*(?:public|internal)\s+(?:(?:sealed|static|abstract|readonly|partial)\s+)*(enum|class|record struct|record|struct|interface)\s+(\w+)", RegexOptions.Multiline)]
    private static partial Regex TypeDeclaration();

    private static bool NotBuildOutput(string path) =>
        !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
        !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}");

    private static string Relative(string path) => Path.GetRelativePath(Root(), path);

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

using System.Text.RegularExpressions;

namespace ArchitectureTests;

/// <summary>
/// Unit and integration tests mirror the modules (module-rules «Test structure»): each file lies under
/// tests/&lt;Project&gt;/&lt;Module | Jobs | Platform | TestSupport&gt;/ and its namespace starts with that folder, so the
/// CI test selection (tools/ci/affected_tests.py) can filter a module's tests by namespace.
/// </summary>
public sealed class TestLayoutTests
{
    private static readonly string[] SharedFolders = ["Jobs", "Platform", "TestSupport"];

    [TestCase("UnitTests")]
    [TestCase("IntegrationTests")]
    public void Test_files_live_in_module_folders_with_matching_namespaces(string project)
    {
        var root = Root();
        var modules = Directory.GetDirectories(Path.Combine(root, "src", "Modules")).Select(Path.GetFileName).ToHashSet();
        var tests = Path.Combine(root, "tests", project);
        var violations = new List<string>();
        foreach (var file in Directory.GetFiles(tests, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(tests, file).Replace('\\', '/');
            var parts = relative.Split('/');
            if (parts[0] is "bin" or "obj")
                continue;
            if (parts.Length < 2 || !(modules.Contains(parts[0]) || SharedFolders.Contains(parts[0])))
            {
                violations.Add($"{relative}: not under a module, Jobs, Platform or TestSupport folder");
                continue;
            }
            var expected = $"{project}.{parts[0]}";
            var match = Regex.Match(File.ReadAllText(file), @"^namespace\s+([\w.]+)", RegexOptions.Multiline);
            if (match.Success && match.Groups[1].Value != expected && !match.Groups[1].Value.StartsWith(expected + ".", StringComparison.Ordinal))
                violations.Add($"{relative}: namespace {match.Groups[1].Value} does not start with {expected}");
        }
        Assert.That(violations, Is.Empty);
    }

    private static string Root()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "BreviERP.sln")))
            directory = directory.Parent;
        Assert.That(directory, Is.Not.Null);
        return directory!.FullName;
    }
}

using System.Text.RegularExpressions;
namespace ArchitectureTests;

/// <summary>Ports and DI chain: each module registers itself through Add&lt;Module&gt;Module(); Api code uses Infrastructure only to register it.</summary>
public sealed class ModuleCompositionTests
{
    private static readonly string[] Modules = ["Accounting", "Ai", "Catalog", "Content", "Crm", "Identity", "Media", "Production", "Purchasing", "Reference", "Reminders", "Warehouse"];

    [TestCaseSource(nameof(Modules))]
    public void Api_registers_the_module_with_one_entry_point(string module)
    {
        var file = Path.Combine(ApiPath(module), "DependencyInjection", $"{module}ModuleExtensions.cs");
        Assert.That(File.Exists(file), Is.True);
        Assert.That(File.ReadAllText(file), Does.Contain($"Add{module}Module(this IServiceCollection services, IConfiguration configuration)"));
    }

    [TestCaseSource(nameof(Modules))]
    public void Api_code_outside_registration_uses_no_infrastructure(string module)
    {
        var root = ApiPath(module);
        var offending = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}DependencyInjection{Path.DirectorySeparatorChar}"))
            .Where(f => Regex.IsMatch(File.ReadAllText(f), @"^using\s+[A-Za-z]+\.Infrastructure\b", RegexOptions.Multiline))
            .Select(f => Path.GetRelativePath(root, f))
            .ToArray();
        Assert.That(offending, Is.Empty);
    }

    [Test]
    public void Host_api_registers_modules_only_through_their_entry_points()
    {
        var host = Path.Combine(Root(), "src", "Bootstrapper", "Host.Api");
        var offending = Directory.GetFiles(host, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(f => Regex.IsMatch(File.ReadAllText(f), @"^using\s+(?!Identity\.)[A-Za-z]+\.Infrastructure\b", RegexOptions.Multiline))
            .Select(f => Path.GetRelativePath(host, f))
            .ToArray();
        Assert.That(offending, Is.Empty);
    }

    private static string ApiPath(string module) => Path.Combine(Root(), "src", "Modules", module, $"{module}.Api", $"{module}.Api");

    private static string Root()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "BreviERP.sln"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        return root!.FullName;
    }
}

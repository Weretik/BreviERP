using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests;

[TestFixture]
public sealed class CatalogImportCommandTests
{
    [TestCase("CatalogImport:ApprovedConnection")]
    [TestCase("CatalogImport:TargetHost")]
    [TestCase("CatalogImport:TargetDatabase")]
    [TestCase("CatalogImport:SourceSha256")]
    public async Task Target_hints_without_import_mode_never_run_migration_or_seed(string hint)
    {
        var services = new NoServices();
        var configuration = Configuration(new() { [hint] = "synthetic" });
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Invoke("SeedJobDispatcher", services, configuration));
        Assert.That(exception!.Message, Is.EqualTo("Catalogue target approval requires the explicit import mode."));
        Assert.That(services.Calls, Is.Zero);
    }

    [TestCase("CatalogImport:ApprovedConnection")]
    [TestCase("CatalogImport:TargetHost")]
    [TestCase("CatalogImport:TargetDatabase")]
    [TestCase("CatalogImport:SourceSha256")]
    public async Task Partial_target_approval_fails_before_reading_source_or_database(string hint)
    {
        var configuration = Configuration(new()
        {
            ["catalog-import"] = "nonexistent-source.json", ["report"] = "nonexistent-report.json",
            [hint] = "synthetic"
        });
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Invoke("CatalogImportCommandRunner", configuration));
        Assert.That(exception!.Message, Is.EqualTo("Catalogue target approval requires host, database and source SHA256."));
    }

    [Test]
    public async Task Report_cannot_replace_source()
    {
        var source = Path.Combine(Path.GetTempPath(), "brevi-catalogue-" + Guid.NewGuid().ToString("N") + ".json");
        var configuration = Configuration(new() { ["catalog-import"] = source, ["report"] = source });
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Invoke("CatalogImportCommandRunner", configuration));
        Assert.That(exception!.Message, Is.EqualTo("Catalogue report must not overwrite the source."));
        Assert.That(File.Exists(source), Is.False);
    }

    [Test]
    public void Secret_bound_approval_creates_only_host_database_hash_metadata()
    {
        var config = Configuration(new()
        {
            ["CatalogImport:ApprovedConnection"] = "Host=cloud.test;Database=approved;Username=synthetic;Password=synthetic",
            ["CatalogImport:SourceSha256"] = new string('a', 64)
        });
        var method = Assembly.Load("Host.Seed").GetType("Host.Seed.Jobs.CatalogImportTargetConfiguration")!
            .GetMethod("Read", BindingFlags.Static | BindingFlags.NonPublic)!;
        var target = (CatalogImport.Execution.CatalogImportTarget)method.Invoke(null, [config])!;
        Assert.That(target.Host, Is.EqualTo("cloud.test"));
        Assert.That(target.Database, Is.EqualTo("approved"));
        Assert.That(target.SourceSha256, Is.EqualTo(new string('a', 64)));
        Assert.That(target.ToString(), Does.Not.Contain("Password").And.Not.Contain("synthetic"));
    }

    [TestCase("catalog-import-abc_01")]
    [TestCase("A_B-09")]
    public void Execution_placeholder_produces_a_unique_report_path(string execution)
    {
        var resolved = ResolveReportPath("/import-output/commit-{execution}.json", execution);
        Assert.That(resolved, Is.EqualTo("/import-output/commit-" + execution + ".json"));
    }

    [TestCase(null)]
    [TestCase("../run")]
    [TestCase("виконання")]
    public void Execution_placeholder_rejects_missing_or_invalid_execution(string? execution)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => ResolveReportPath("/import-output/commit-{execution}.json", execution));
        Assert.That(exception!.Message, Is.EqualTo("Catalogue report placeholder requires a valid CLOUD_RUN_EXECUTION."));
    }

    [Test]
    public void Fixed_report_path_is_unchanged_without_execution_metadata()
    {
        var path = Path.Combine(Path.GetTempPath(), "catalog-import-" + Guid.NewGuid().ToString("N") + ".json");
        Assert.That(ResolveReportPath(path, null), Is.EqualTo(path));
        Assert.That(ResolveReportPath(path, "../ignored"), Is.EqualTo(path));
    }

    [Test]
    public void Execution_identifier_has_an_explicit_length_bound()
    {
        Assert.That(ResolveReportPath("{execution}.json", new string('a', 128)), Is.EqualTo(new string('a', 128) + ".json"));
        Assert.Throws<InvalidOperationException>(() => ResolveReportPath("{execution}.json", new string('a', 129)));
        Assert.Throws<InvalidOperationException>(() => ResolveReportPath("{execution}.json", ""));
    }

    private static string ResolveReportPath(string path, string? execution)
    {
        var method = Assembly.Load("Host.Seed").GetType("Host.Seed.Jobs.CatalogImportReportPath")!
            .GetMethod("Resolve", BindingFlags.Static | BindingFlags.Public)!;
        try { return (string)method.Invoke(null, [path, execution])!; }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static Task Invoke(string type, params object[] arguments)
    {
        var method = Assembly.Load("Host.Seed").GetType("Host.Seed.Jobs." + type)!
            .GetMethod("RunAsync", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        try { return (Task)method.Invoke(null, arguments)!; }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private sealed class NoServices : IServiceProvider
    {
        internal int Calls { get; private set; }
        public object? GetService(Type serviceType)
        {
            Calls++;
            throw new InvalidOperationException("Unexpected service resolution.");
        }
    }
}

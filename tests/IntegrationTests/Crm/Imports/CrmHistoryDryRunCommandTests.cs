using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Crm.Imports;

[TestFixture]
public sealed class CrmHistoryDryRunCommandTests
{
    private string root = null!;
    private string source = null!;
    private string freeze = null!;
    private string report = null!;
    private readonly NoServices services = new();

    [SetUp]
    public void SetUp()
    {
        services.Calls = 0;
        root = Path.Combine(Path.GetTempPath(), "brevi-p7-host-" + Guid.NewGuid().ToString("N"));
        source = Path.Combine(root, "source");
        Directory.CreateDirectory(source);
        report = Path.Combine(root, "output", "report.json");
        var tables = new[] { "counterparties", "quotes", "quote_versions", "quote_items", "quote_change_log", "quote_transfers",
            "quote_email_sends", "quote_ppe_price_overrides", "custom_sewing_drafts", "custom_sewing_versions", "custom_sewing_files", "introduction_saved_templates" };
        var database = tables.ToDictionary(x => x, x => new { columns = new[] { "id" }, rows = Array.Empty<object>() });
        var manifestTables = tables.ToDictionary(x => x, x => new
        {
            status = "all_available_rows_read",
            rows_exported = 0,
            reached_last_page = true,
            truncated_pages = Array.Empty<object>(),
            redacted_fields = Array.Empty<string>(),
            issues = Array.Empty<string>(),
            columns_match_source_schema = true
        });
        var dbPath = Path.Combine(source, "database.json");
        var manifestPath = Path.Combine(source, "manifest.json");
        File.WriteAllText(dbPath, JsonSerializer.Serialize(database));
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(new
        {
            full_backup = false,
            transaction_consistent_snapshot = false,
            schema_source_version = 211,
            total_rows_exported = 0,
            tables_discovered = tables.Length,
            read_started_at = "2026-11-02T00:00:00Z",
            read_finished_at = "2026-11-02T00:01:00Z",
            tables = manifestTables
        }));
        freeze = Path.Combine(source, "freeze.json");
        File.WriteAllText(freeze, JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            datasetId = "host-fixture",
            databaseFile = "database.json",
            databaseSha256 = Hash(dbPath),
            manifestFile = "manifest.json",
            manifestSha256 = Hash(manifestPath)
        }));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }

    [Test]
    public async Task DryRun_UsesNoServicesOrDatabase_AndWritesOnlyReportAndDatasetLedger()
    {
        var ledger = Path.Combine(root, "output", "ledger.json");
        var config = Configuration(new() { ["staging-ledger"] = ledger });
        var databaseHash = Hash(Path.Combine(source, "database.json"));
        await Dispatch(config);
        using var json = JsonDocument.Parse(await File.ReadAllTextAsync(report));
        Assert.That(json.RootElement.GetProperty("mode").GetString(), Is.EqualTo("dry-run"));
        Assert.That(json.RootElement.GetProperty("databaseWrites").GetInt32(), Is.Zero);
        Assert.That(json.RootElement.GetProperty("importAuthorized").GetBoolean(), Is.False);
        Assert.That(json.RootElement.GetProperty("controlSha256").GetString(), Is.EqualTo(Hash(freeze)));
        Assert.That(json.RootElement.GetProperty("stagingLedger").GetArrayLength(), Is.Zero);
        Assert.That(services.Calls, Is.Zero);
        Assert.That(Hash(Path.Combine(source, "database.json")), Is.EqualTo(databaseHash));
        using var receipt = JsonDocument.Parse(await File.ReadAllTextAsync(ledger));
        Assert.That(receipt.RootElement.GetProperty("datasetId").GetString(), Is.EqualTo("host-fixture"));
    }

    [Test]
    public async Task ConflictingModes_AreRejectedBeforeAnyServiceOrOutput()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(Configuration(new() { ["crm-migrate"] = "plan" })));
        Assert.That(error!.Message, Is.EqualTo("Select exactly one explicit seed job mode."));
        Assert.That(services.Calls, Is.Zero);
        Assert.That(File.Exists(report), Is.False);
    }

    [Test]
    public async Task ApplyMode_IsRejectedWithoutDatabaseAccessOrOutput()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(Configuration(new() { ["mode"] = "apply" })));
        Assert.That(error!.Message, Is.EqualTo("CRM history mode is read-only dry-run."));
        Assert.That(services.Calls, Is.Zero);
        Assert.That(File.Exists(report), Is.False);
    }

    [Test]
    public async Task ExistingReport_IsNeverOverwritten()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(report)!);
        File.WriteAllText(report, "keep-existing");
        await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(Configuration(new())));
        Assert.That(File.ReadAllText(report), Is.EqualTo("keep-existing"));
        Assert.That(services.Calls, Is.Zero);
    }

    [Test]
    public async Task InputOrGitOutput_IsRejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(Configuration(new() { ["report"] = Path.Combine(source, "manifest.json") })));
        var repo = Path.Combine(root, "fake-repo");
        Directory.CreateDirectory(Path.Combine(repo, ".git"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(Configuration(new() { ["report"] = Path.Combine(repo, "report.json") })));
        Assert.That(services.Calls, Is.Zero);
    }

    [Test]
    public async Task HistoryPathsWithoutExplicitMode_NeverFallThroughToNormalMigrations()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(Configuration(new() { ["crm-history-dry-run"] = null })));
        Assert.That(error!.Message, Is.EqualTo("CRM history paths require the explicit dry-run mode."));
        Assert.That(services.Calls, Is.Zero);
        Assert.That(File.Exists(report), Is.False);
    }

    private IConfiguration Configuration(Dictionary<string, string?> extras)
    {
        var values = new Dictionary<string, string?>
        {
            ["crm-history-dry-run"] = freeze,
            ["source-root"] = source,
            ["report"] = report,
            ["ConnectionStrings:Default"] = "Host=127.0.0.1;Port=1;Database=unavailable;Username=synthetic;Password=synthetic"
        };
        foreach (var pair in extras) values[pair.Key] = pair.Value;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private async Task Dispatch(IConfiguration configuration)
    {
        var assembly = Assembly.Load("Host.Seed");
        var type = assembly.GetType("Host.Seed.Jobs.SeedJobDispatcher", throwOnError: true)!;
        var method = type.GetMethod("RunAsync", BindingFlags.Public | BindingFlags.Static)!;
        await (Task)method.Invoke(null, [services, configuration])!;
    }

    private static string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private sealed class NoServices : IServiceProvider
    {
        public int Calls { get; set; }
        public object? GetService(Type serviceType)
        {
            Calls++;
            throw new InvalidOperationException("Unexpected service resolution.");
        }
    }
}

using CatalogImport.Execution;
using Npgsql;

namespace UnitTests;

public sealed class CatalogImportTargetTests
{
    private const string Hash = "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF";

    [TestCase("localhost")]
    [TestCase("127.0.0.1")]
    [TestCase("::1")]
    public void Default_target_keeps_existing_loopback_behavior(string host) => Assert.DoesNotThrow(() => Verify(host, "brevidb", Hash, null));

    [Test]
    public void Default_target_rejects_remote_connection() => Assert.Throws<InvalidOperationException>(() => Verify("remote.example", "brevidb", Hash, null));

    [TestCase("remote.example")]
    [TestCase("/cloudsql/project:region:instance")]
    public void Explicit_remote_target_requires_matching_database_and_frozen_source(string host) => Assert.DoesNotThrow(() => Verify(host, "brevidb", Hash, new CatalogImportTarget(host, "brevidb", Hash)));

    [Test]
    public void Host_and_hash_allow_case_but_database_is_exact()
    {
        Assert.DoesNotThrow(() => Verify("REMOTE.EXAMPLE", "brevidb", Hash.ToLowerInvariant(), new CatalogImportTarget("remote.example", "brevidb", Hash)));
        Assert.Throws<InvalidOperationException>(() => Verify("remote.example", "BreviDb", Hash, new CatalogImportTarget("remote.example", "brevidb", Hash)));
    }

    [TestCase("other.example", "brevidb")]
    [TestCase("remote.example", "otherdb")]
    public void Explicit_target_rejects_a_different_connection(string host, string database) => Assert.Throws<InvalidOperationException>(() => Verify(host, database, Hash, new CatalogImportTarget("remote.example", "brevidb", Hash)));

    [TestCase(null, "brevidb", Hash)]
    [TestCase("", "brevidb", Hash)]
    [TestCase(" ", "brevidb", Hash)]
    [TestCase("remote.example", null, Hash)]
    [TestCase("remote.example", "", Hash)]
    [TestCase("remote.example", " ", Hash)]
    [TestCase("remote.example", "brevidb", null)]
    [TestCase("remote.example", "brevidb", "")]
    [TestCase("remote.example", "brevidb", "0123")]
    [TestCase("remote.example", "brevidb", "Z123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF")]
    public void Explicit_target_rejects_missing_or_malformed_approval_fields(string? host, string? database, string? sourceHash) => Assert.Throws<InvalidOperationException>(() => Verify("remote.example", "brevidb", Hash, new CatalogImportTarget(host!, database!, sourceHash!)));

    [TestCase("localhost")]
    [TestCase("remote.example")]
    public void Explicit_target_rejects_changed_frozen_source_even_on_loopback(string host) => Assert.Throws<InvalidOperationException>(() => Verify(host, "brevidb", new string('F', 64), new CatalogImportTarget(host, "brevidb", Hash)));

    [Test]
    public void Rejection_does_not_expose_connection_secrets_or_requested_values()
    {
        var settings = Settings("other.example", "private-db");
        settings.Password = "private-password";
        var exception = Assert.Throws<InvalidOperationException>(() => CatalogImportTargetGuard.ValidateConnection(settings, new CatalogImportTarget("remote.example", "brevidb", Hash)));
        Assert.That(exception!.Message, Does.Not.Contain("private-password").And.Not.Contain("private-db").And.Not.Contain("other.example"));
    }

    [Test]
    public async Task Invalid_mode_is_rejected_before_source_read_or_connection_open()
    {
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() => CatalogImportJob.RunAsync("not-a-connection", "missing-source.json", "unexpected"));
        Assert.That(exception!.Message, Is.EqualTo("Import mode must be validate, trial or apply."));
    }

    [Test]
    public async Task Job_rejects_target_mismatch_before_loading_source()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CatalogImportJob.RunAsync(
            Settings("other.example", "brevidb").ConnectionString, "missing-source.json", "validate",
            approvedTarget: new CatalogImportTarget("remote.example", "brevidb", Hash)));
        Assert.That(exception!.Message, Is.EqualTo("The catalogue import target does not match its explicit approval."));
    }

    [Test]
    public async Task Job_rejects_hash_mismatch_before_opening_connection()
    {
        var settings = Settings("127.0.0.1", "synthetic-unavailable");
        settings.Port = 1;
        settings.Timeout = 1;
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => CatalogImportJob.RunAsync(
            settings.ConnectionString, Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "catalog-import.json"), "validate",
            approvedTarget: new CatalogImportTarget(settings.Host!, settings.Database!, Hash)));
        Assert.That(exception!.Message, Is.EqualTo("The catalogue import source does not match its explicit approval."));
    }

    [TestCase("")]
    [TestCase("0123")]
    [TestCase("Z123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF")]
    public void Explicit_target_rejects_invalid_observed_source_hash(string sourceHash)
    {
        Assert.Throws<InvalidOperationException>(() => Verify("localhost", "brevidb", sourceHash, new CatalogImportTarget("localhost", "brevidb", Hash)));
    }

    private static void Verify(string host, string database, string sourceHash, CatalogImportTarget? target)
    {
        CatalogImportTargetGuard.ValidateConnection(Settings(host, database), target);
        CatalogImportTargetGuard.ValidateSourceHash(sourceHash, target);
    }

    private static NpgsqlConnectionStringBuilder Settings(string host, string database) => new() { Host = host, Database = database, Username = "synthetic" };
}

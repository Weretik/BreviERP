using System.Text.Json;
using Crm.Infrastructure.Imports.History.Source;

namespace IntegrationTests.Crm.Imports;

[TestFixture]
public sealed partial class CrmHistorySourceReaderTests
{
    private readonly CrmHistorySourceReader reader = new();

    [Test]
    public async Task Reader_NormalizesOnlyAllowedMetadataAndHashesIdentityWithoutPrivateCells()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["counterparties"].Add(new { id = "client_1", name = "PRIVATE_NAME", iban = "PRIVATE_IBAN", roles_json = "[\"CLIENT\",\"SUPPLIER\"]", external_source = "CRM", external_type = "Company", external_id = "Company_1" });
        fixture.Rows["custom_sewing_drafts"].Add(new { id = "draft_1", owner_email = " Source@Example.Test ", payload_json = "{}" });
        fixture.Write();
        var result = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(result.Snapshot.Tables, Has.Count.EqualTo(12));
        Assert.That(result.Snapshot.Rows, Has.Count.EqualTo(2));
        Assert.That(result.Snapshot.Rows.Single(x => x.Entity == "counterparties").Roles, Is.EquivalentTo(new[] { "CLIENT", "SUPPLIER" }));
        Assert.That(result.Snapshot.Rows.All(x => x.OwnerKeys.All(k => k.Length == 64)), Is.True);
        Assert.That(result.Snapshot.Rows.Single(x => x.Entity == "counterparties").ExternalIdentityKeys, Has.Count.EqualTo(1));
        Assert.That(result.Options.ScopeApprovalReference, Is.Null);
        var serialized = JsonSerializer.Serialize(result);
        Assert.That(serialized, Does.Not.Contain("PRIVATE"));
        Assert.That(serialized, Does.Not.Contain("Source@"));
        Assert.That(serialized, Does.Not.Contain("gmail_connections"));
        Assert.That(serialized, Does.Not.Contain("transactions"));
    }

    [Test]
    public async Task Reader_PreservesPartialEvidenceAndNeverRepairsTruncatedSnapshots()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["quote_versions"].Add(new { id = "v1", quote_id = "q1", version_number = 1, snapshot_json = "{\"private\":\"[truncated 100 chars]", changes_json = "[]", created_by_email = "source@example.test" });
        fixture.Write(complete: false);
        var result = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(result.Snapshot.FullBackup, Is.False);
        Assert.That(result.Snapshot.TransactionConsistent, Is.False);
        Assert.That(result.Snapshot.Tables.Single(x => x.Entity == "quote_versions").Complete, Is.False);
        Assert.That(result.Snapshot.Rows.Single().Issues, Does.Contain("truncated_json"));
        Assert.That(result.Snapshot.Rows.Single().Issues, Does.Contain("invalid_snapshot_json"));
    }

    [TestCase("../outside.json")]
    [TestCase("https://example.test/private.json")]
    [TestCase("C:\\private.json")]
    public async Task Reader_RejectsReferencedPathEscapes(string file)
    {
        using var fixture = new CrmHistorySourceFixture(); fixture.Write();
        fixture.Freeze["databaseFile"] = file; fixture.SaveFreeze();
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
        Assert.That(error!.Message, Is.EqualTo("crm_history_source_invalid"));
    }

    [Test]
    public async Task Reader_VerifiesHashesBeforeParsing()
    {
        using var fixture = new CrmHistorySourceFixture(); fixture.Write();
        File.WriteAllText(Path.Combine(fixture.Root, "database.json"), "PRIVATE_INVALID_JSON");
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
        Assert.That(error!.Message, Is.EqualTo("crm_history_source_invalid"));
    }

    [Test]
    public async Task Reader_RejectsUnknownFreezeAndOptionsProperties()
    {
        using var fixture = new CrmHistorySourceFixture(); fixture.Write();
        fixture.Freeze["sendEmail"] = true; fixture.SaveFreeze();
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
        fixture.Write(optionsJson: "{\"ownerMappings\":{},\"exactIdentityMappings\":{},\"previousStaging\":[],\"scopeApprovalReference\":null,\"retentionPolicyReference\":null,\"unexpected\":true}");
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
    }

    [Test]
    public async Task Reader_StopsForCountMismatchAndOversizedArtifact()
    {
        using var fixture = new CrmHistorySourceFixture(); fixture.Write(expectedCounts: new() { ["quotes"] = 1 });
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
        fixture.Write();
        using (var file = new FileStream(Path.Combine(fixture.Root, "database.json"), FileMode.Create)) file.SetLength(64L * 1024 * 1024 + 1);
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
    }

    [Test]
    public async Task Reader_RecordsMissingFileBytesAndHashesBadSourceIdsWithoutEcho()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["custom_sewing_files"].Add(new { id = "source-person@example.test", owner_email = "source@example.test", size = 4, mime_type = "application/pdf" });
        fixture.Write(); var result = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        var row = result.Snapshot.Rows.Single();
        Assert.That(row.SourceId, Does.StartWith("invalid_"));
        Assert.That(row.Issues, Does.Contain("invalid_source_id"));
        Assert.That(row.Issues, Does.Contain("missing_file_bytes"));
        Assert.That(JsonSerializer.Serialize(result), Does.Not.Contain("source-person@"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Reader_VerifiesFrozenBlobBytesAndNativeSize(bool mismatch)
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["custom_sewing_files"].Add(new { id = "file_1", owner_email = "source@example.test", size = mismatch ? 5 : 4, mime_type = "application/pdf" });
        fixture.Write(); File.WriteAllBytes(Path.Combine(fixture.Root, "proof.bin"), [1, 2, 3, 4]);
        fixture.Freeze["blobs"] = new[] { new { sourceFileId = "file_1", file = "proof.bin", sha256 = fixture.HashFile("proof.bin"), length = 4 } }; fixture.SaveFreeze();
        var result = await reader.ReadAsync(fixture.FreezePath, fixture.Root); var row = result.Snapshot.Rows.Single();
        Assert.That(row.Issues.Contains("invalid_file_metadata"), Is.EqualTo(mismatch));
        Assert.That(row.Issues, Does.Not.Contain("missing_file_bytes"));
    }
}

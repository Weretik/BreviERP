using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace IntegrationTests.Crm.Imports;

public sealed partial class CrmHistorySourceReaderTests
{
    private static string HashText(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    [Test]
    public async Task Reader_SealsControlAndOptionsAndUsesExactOwnerAndIdentityHashes()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["counterparties"].Add(new { id = "client_1", roles_json = "[\"CLIENT\"]", external_source = "CRM", external_type = "Company", external_id = "Company_1" });
        fixture.Rows["custom_sewing_drafts"].Add(new { id = "draft_1", owner_email = " Source@Example.Test ", payload_json = "{}" });
        fixture.Write(optionsJson: JsonSerializer.Serialize(new { ownerMappings = new Dictionary<string, int>(), exactIdentityMappings = new Dictionary<string, string>(), previousStaging = Array.Empty<object>(), scopeApprovalReference = (string?)null, retentionPolicyReference = "owner-no-auto-delete-2026-11-02", stagingDatasetId = (string?)null }));
        var result = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(result.Snapshot.ControlSha256, Is.EqualTo(fixture.HashFile("freeze.json")));
        Assert.That(result.Snapshot.OptionsSha256, Is.EqualTo(fixture.HashFile("options.json")));
        Assert.That(result.Options.RetentionPolicyReference, Is.EqualTo("owner-no-auto-delete-2026-11-02"));
        var client = result.Snapshot.Rows.Single(x => x.Entity == "counterparties");
        Assert.That(client.OwnerKeys, Is.EqualTo(new[] { HashText("counterparty:client_1") }));
        Assert.That(client.ExternalIdentityKeys, Is.EqualTo(new[] { HashText(JsonSerializer.Serialize(new[] { "CRM", "Company", "Company_1" })) }));
        Assert.That(result.Snapshot.Rows.Single(x => x.Entity == "custom_sewing_drafts").OwnerKeys, Is.EqualTo(new[] { HashText("source@example.test") }));
    }

    [Test]
    public async Task Reader_RejectsDuplicateControlAndOptionProperties()
    {
        using var fixture = new CrmHistorySourceFixture(); fixture.Write();
        File.WriteAllText(fixture.FreezePath, File.ReadAllText(fixture.FreezePath).Replace("\"schemaVersion\":1", "\"schemaVersion\":1,\"schemaVersion\":1", StringComparison.Ordinal));
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
        var hash = new string('a', 64);
        fixture.Write(optionsJson: "{\"ownerMappings\":{\"" + hash + "\":1,\"" + hash + "\":2},\"exactIdentityMappings\":{},\"previousStaging\":[],\"scopeApprovalReference\":null,\"retentionPolicyReference\":null}");
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
    }

    [Test]
    public async Task Reader_ReportsBlobHashMismatchWithoutPretendingBytesAreVerified()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["custom_sewing_files"].Add(new { id = "file_1", owner_email = "source@example.test", size = 4, mime_type = "application/pdf" });
        fixture.Write(); File.WriteAllBytes(Path.Combine(fixture.Root, "proof.bin"), [1, 2, 3, 4]);
        fixture.Freeze["blobs"] = new[] { new { sourceFileId = "file_1", file = "proof.bin", sha256 = new string('0', 64), length = 4 } }; fixture.SaveFreeze();
        var request = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(request.Snapshot.Rows.Single().Issues, Does.Contain("invalid_file_metadata"));
    }

    [Test]
    public async Task Reader_RejectsFrozenBlobPathEscapeBeforeReading()
    {
        using var fixture = new CrmHistorySourceFixture(); fixture.Write();
        fixture.Freeze["blobs"] = new[] { new { sourceFileId = "file_1", file = "../outside.bin", sha256 = new string('0', 64), length = 4 } }; fixture.SaveFreeze();
        var error = await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
        Assert.That(error!.Message, Is.EqualTo("crm_history_source_invalid"));
    }

    [Test]
    public async Task Reader_ExtractsOnlyProvenGraphFieldsAndReverseQuoteMembership()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["quotes"].Add(new { id = "q1", counterparty_id = "c1", current_version_number = 1, created_by_email = "source@example.test", current_responsible_email = "source@example.test" });
        fixture.Rows["quote_versions"].Add(new { id = "v1", quote_id = "q1", version_number = 1, snapshot_json = JsonSerializer.Serialize(new { items = new object[] { new { } } }), changes_json = "[]", created_by_email = "source@example.test" });
        fixture.Rows["quote_items"].Add(new { id = "i1", quote_version_id = "v1", item_kind = "custom_sewing", snapshot_json = JsonSerializer.Serialize(new { customVersionId = "cv1", customFabricVariants = new[] { new { customVersionId = "cv2" } } }) });
        fixture.Rows["custom_sewing_versions"].Add(new { id = "cv1", owner_email = "source@example.test", payload_json = JsonSerializer.Serialize(new { input = new { files = new[] { new { id = "file_1" } }, applications = new[] { new { artworkId = "file_1" } } } }) });
        fixture.Write(); var request = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(request.Snapshot.Rows.Single(x => x.SourceId == "q1").References.Any(x => x.Entity == "quote_versions" && x.SourceId == "v1"), Is.True);
        Assert.That(request.Snapshot.Rows.Single(x => x.SourceId == "v1").References.Any(x => x.Entity == "quote_items" && x.SourceId == "i1"), Is.True);
        var item = request.Snapshot.Rows.Single(x => x.SourceId == "i1");
        Assert.That(item.References.Where(x => x.Entity == "custom_sewing_versions").Select(x => x.SourceId), Is.EquivalentTo(new[] { "cv1", "cv2" }));
        Assert.That(request.Snapshot.Rows.Single(x => x.SourceId == "cv1").References.Single().SourceId, Is.EqualTo("file_1"));
    }

    [Test]
    public async Task Reader_DoesNotGuessUnknownPayloadOrLegacyIntroductionTargets()
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["custom_sewing_drafts"].Add(new { id = "draft_1", owner_email = "source@example.test", payload_json = JsonSerializer.Serialize(new { arbitrary = new { id = "PRIVATE_FAKE_FILE" } }) });
        fixture.Rows["introduction_saved_templates"].Add(new { id = "intro_1", created_by_email = "source@example.test", snapshot_json = JsonSerializer.Serialize(new { template = "summer", selections = Enumerable.Range(1, 4).Select(x => new { uid = 100 + x, productId = "product_" + x, fabricId = "fabric_" + x }), emailTemplateId = "legacy_email" }) });
        fixture.Write(); var request = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(request.Snapshot.Rows.Single(x => x.SourceId == "draft_1").Issues, Does.Contain("unsupported_payload_shape"));
        Assert.That(request.Snapshot.Rows.Single(x => x.SourceId == "draft_1").References, Is.Empty);
        Assert.That(request.Snapshot.Rows.Single(x => x.SourceId == "intro_1").Issues, Does.Contain("legacy_target_reference_requires_mapping"));
        Assert.That(request.Snapshot.Rows.Single(x => x.SourceId == "intro_1").References, Is.Empty);
        Assert.That(JsonSerializer.Serialize(request), Does.Not.Contain("PRIVATE_FAKE_FILE"));
    }

    [Test]
    public async Task Reader_PreservesMalformedRowCountWithoutEchoingTheRawValue()
    {
        using var fixture = new CrmHistorySourceFixture(); fixture.Rows["counterparties"].Add("PRIVATE_BAD_ROW"); fixture.Write();
        var request = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(request.Snapshot.Rows, Has.Count.EqualTo(1));
        Assert.That(request.Snapshot.Rows.Single().Issues, Does.Contain("invalid_row_shape"));
        Assert.That(JsonSerializer.Serialize(request), Does.Not.Contain("PRIVATE_BAD_ROW"));
    }

    [Test]
    public async Task Reader_RejectsSymbolicLinkEscapesWhenThePlatformAllowsCreation()
    {
        using var outside = new CrmHistorySourceFixture(); outside.Write();
        using var fixture = new CrmHistorySourceFixture(); fixture.Write();
        try { File.CreateSymbolicLink(Path.Combine(fixture.Root, "link.json"), Path.Combine(outside.Root, "database.json")); }
        catch (Exception error) when (error is UnauthorizedAccessException or PlatformNotSupportedException or IOException)
        { Assert.Ignore("Platform does not permit creating a synthetic symbolic link."); }
        fixture.Freeze["databaseFile"] = "link.json"; fixture.Freeze["databaseSha256"] = outside.HashFile("database.json"); fixture.SaveFreeze();
        await Assert.ThrowsAsync<InvalidDataException>(() => reader.ReadAsync(fixture.FreezePath, fixture.Root));
    }
    [TestCase("sewing")]
    [TestCase("ppe")]
    public async Task Reader_DoesNotTreatLegacyCatalogPayloadAsReviewedTargetMapping(string kind)
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["quote_items"].Add(new { id = "i1", quote_version_id = "v1", item_kind = kind, snapshot_json = "{}" });
        fixture.Write(); var request = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        Assert.That(request.Snapshot.Rows.Single().Issues, Does.Contain("legacy_target_reference_requires_mapping"));
    }

    [TestCase(0)]
    [TestCase(1)]
    public async Task Reader_RequiresPositiveCurrentVersionAndReportsAmbiguousVersionNumbers(int currentVersion)
    {
        using var fixture = new CrmHistorySourceFixture();
        fixture.Rows["quotes"].Add(new { id = "q1", current_version_number = currentVersion });
        foreach (var id in new[] { "v1", "v2" }) fixture.Rows["quote_versions"].Add(new { id, quote_id = "q1", version_number = 1, snapshot_json = JsonSerializer.Serialize(new { items = Array.Empty<object>() }), changes_json = "[]" });
        fixture.Write(); var request = await reader.ReadAsync(fixture.FreezePath, fixture.Root);
        var quote = request.Snapshot.Rows.Single(x => x.SourceId == "q1");
        Assert.That(quote.Issues, Does.Contain("ambiguous_quote_version"));
        Assert.That(quote.Issues, Does.Contain("missing_current_version"));
        if (currentVersion == 0) Assert.That(quote.Issues, Does.Contain("unsupported_payload_shape"));
        Assert.That(request.Snapshot.Rows.Where(x => x.Entity == "quote_versions").All(x => x.Issues.Contains("ambiguous_quote_version")), Is.True);
    }

}

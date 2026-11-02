using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Imports.History.Contracts;
using Crm.Application.Imports.History.Planning;
using static UnitTests.Crm.Imports.CrmHistoryPlannerFixture;
namespace UnitTests.Crm.Imports;

public sealed class CrmHistoryDryRunPlannerTests
{
    private readonly CrmHistoryDryRunPlanner planner = new();
    private CrmHistoryDryRunReport Run(CrmHistoryDryRunRequest request)
    {
        var result = planner.Plan(request);
        Assert.That(result.IsSuccess, Is.True);
        return result.Value;
    }

    [Test]
    public void Candidate_plan_remains_non_authorizing_and_preserves_source_provenance()
    {
        var input = Request(Row());
        input = input with { Snapshot = input.Snapshot with { ControlSha256 = Hash('f'), OptionsSha256 = Hash('1') } };
        var report = Run(input);
        Assert.Multiple(() =>
        {
            Assert.That(report.Mode, Is.EqualTo("dry-run")); Assert.That(report.DatabaseWrites, Is.Zero);
            Assert.That(report.ImportAuthorized, Is.False); Assert.That(report.Status, Is.EqualTo("ready_for_review"));
            Assert.That(report.Counts.SourceRows, Is.EqualTo(1)); Assert.That(report.Counts.WouldImport, Is.EqualTo(1));
            Assert.That(report.ControlSha256, Is.EqualTo(Hash('f'))); Assert.That(report.OptionsSha256, Is.EqualTo(Hash('1')));
        });
    }

    [Test]
    public void Explicit_client_dual_supplier_finance_and_unknown_roles_have_disjoint_outcomes()
    {
        var report = Run(Request(Row("client"), Row("dual") with { Roles = ["CLIENT", "SUPPLIER"] },
            Row("supplier") with { Roles = ["SUPPLIER"] }, Row("finance") with { Roles = ["FINANCIAL"] },
            Row("unknown") with { Roles = [] }));
        Assert.That(report.Counts, Is.EqualTo(new CrmHistoryDryRunCounts(5, 2, 0, 0, 1, 2, 0)));
    }

    [Test]
    public void Exact_supplied_identity_mapping_is_preferred_to_candidate_or_staging_replay()
    {
        var row = Row() with { ExternalIdentityKeys = [IdentityHash] }; var request = Request(row);
        request = request with
        {
            Options = request.Options with
            {
                ExactIdentityMappings = new Dictionary<string, string> { { IdentityHash, Guid.NewGuid().ToString() } },
                PreviousStaging = [new(row.Entity, row.SourceId, row.PayloadSha256)],
                StagingDatasetId = request.Snapshot.DatasetId
            }
        };
        var report = Run(request);
        Assert.That(report.Counts.AlreadyMapped, Is.EqualTo(1)); Assert.That(report.Counts.WouldImport, Is.Zero);
    }

    [Test]
    public void Divergent_exact_identity_maps_are_review_conflicts()
    {
        var row = Row() with { ExternalIdentityKeys = [IdentityHash, Hash('2')] }; var request = Request(row);
        request = request with { Options = request.Options with { ExactIdentityMappings = new Dictionary<string, string> { { IdentityHash, Guid.NewGuid().ToString() }, { Hash('2'), Guid.NewGuid().ToString() } } } };
        var report = Run(request);
        Assert.That(report.Rows[0].Status, Is.EqualTo("needsReview")); Assert.That(report.Rows[0].Reasons, Contains.Item("exact_identity_conflict"));
    }

    [Test]
    public void Missing_owner_scope_and_retention_are_visible_without_invented_approvals()
    {
        var input = Request(Row()); input = input with { Options = input.Options with { OwnerMappings = new Dictionary<string, int>(), ScopeApprovalReference = null, RetentionPolicyReference = null } };
        var report = Run(input);
        Assert.That(report.Status, Is.EqualTo("blocked")); Assert.That(report.Rows[0].Reasons, Contains.Item("owner_unmapped"));
        Assert.That(report.Blockers, Contains.Item("scope_approval_missing")); Assert.That(report.Blockers, Contains.Item("retention_policy_missing"));
    }

    [TestCase("sewing")]
    [TestCase("ppe")]
    [TestCase("one_off_ppe")]
    [TestCase("custom_sewing")]
    public void Actual_supported_item_kinds_are_candidates(string kind)
        => Assert.That(Run(Request(Row("item", "quote_items", kind))).Counts.WouldImport, Is.EqualTo(1));

    [TestCase("individual")]
    [TestCase("catalog")]
    [TestCase(null)]
    public void Legacy_or_missing_item_kind_is_review_only(string? kind)
        => Assert.That(Run(Request(Row("item", "quote_items", kind))).Counts.NeedsReview, Is.EqualTo(1));

    [TestCase("invalid_snapshot_json")]
    [TestCase("truncated_json")]
    [TestCase("invalid_roles_json")]
    public void Invalid_source_cells_are_quarantined_without_staging_ledger_entries(string issue)
    {
        var report = Run(Request(Row() with { Issues = [issue] }));
        Assert.That(report.Counts.Invalid, Is.EqualTo(1)); Assert.That(report.StagingLedger, Is.Empty);
    }

    [Test]
    public void Unknown_source_issue_text_and_unsafe_source_ids_do_not_leak_into_reports()
    {
        var secret = "private@example.test";
        var report = Run(Request(Row("safe") with { Issues = [secret] }, Row(secret)));
        Assert.That(report.Counts.NeedsReview, Is.EqualTo(1)); Assert.That(report.Counts.Invalid, Is.EqualTo(1));
        Assert.That(JsonSerializer.Serialize(report), Does.Not.Contain(secret));
        Assert.That(report.Rows.All(x => x.SourceKeySha256.Length == 64), Is.True);
        Assert.That(report.StagingLedger, Has.Count.EqualTo(1));
    }

    [Test]
    public void Table_counts_and_incomplete_source_flags_block_the_plan_without_claiming_completeness()
    {
        var request = Request(Row()); request = request with
        {
            Snapshot = request.Snapshot with
            {
                FullBackup = false,
                TransactionConsistent = false,
                Tables = [new("counterparties", 99, 1, false, ["do_not_echo_private_detail"])]
            }
        };
        var report = Run(request);
        Assert.That(report.Status, Is.EqualTo("blocked")); Assert.That(report.Blockers, Contains.Item("source_table_count_mismatch"));
        Assert.That(JsonSerializer.Serialize(report), Does.Not.Contain("do_not_echo_private_detail"));
    }

    [Test]
    public void Missing_parent_review_propagates_through_the_whole_graph()
    {
        var quote = Row("q", "quotes") with { References = [new("counterparties", "missing")] };
        var version = Row("v", "quote_versions") with { References = [new("quotes", "q")] };
        var item = Row("i", "quote_items", "sewing") with { References = [new("quote_versions", "v")] };
        var report = Run(Request(quote, version, item));
        Assert.That(report.Counts.NeedsReview, Is.EqualTo(3)); Assert.That(report.Counts.WouldImport, Is.Zero);
    }

    [Test]
    public void Invalid_parent_does_not_make_its_valid_children_import_candidates()
    {
        var report = Run(Request(Row("client") with { Issues = ["invalid_row_shape"] },
            Row("q", "quotes") with { References = [new("counterparties", "client")] }));
        Assert.That(report.Counts.Invalid, Is.EqualTo(1)); Assert.That(report.Counts.NeedsReview, Is.EqualTo(1));
    }

    [Test]
    public void Same_key_hash_duplicates_are_one_candidate_and_one_staging_noop()
    {
        var row = Row(); var report = Run(Request(row, row));
        Assert.That(report.Counts, Is.EqualTo(new CrmHistoryDryRunCounts(2, 1, 1, 0, 0, 0, 0)));
        Assert.That(report.StagingLedger, Has.Count.EqualTo(1));
    }

    [Test]
    public void Changed_hash_duplicates_are_conflicts_for_every_row_in_the_group()
    {
        var row = Row(); var report = Run(Request(row, row with { PayloadSha256 = Hash('3') }));
        Assert.That(report.Counts.NeedsReview, Is.EqualTo(2)); Assert.That(report.StagingLedger, Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Prior_staging_is_a_noop_only_for_the_same_dataset_key_and_hash(bool changed)
    {
        var row = Row(); var request = Request(row);
        request = request with { Options = request.Options with { PreviousStaging = [new(row.Entity, row.SourceId, changed ? Hash('3') : row.PayloadSha256)], StagingDatasetId = request.Snapshot.DatasetId } };
        var report = Run(request);
        Assert.That(changed ? report.Counts.NeedsReview : report.Counts.AlreadyStaged, Is.EqualTo(1));
        Assert.That(report.Counts.AlreadyMapped, Is.Zero);
    }

    [Test]
    public void Staging_from_another_dataset_is_rejected_without_exposing_it()
    {
        var row = Row(); var request = Request(row);
        request = request with { Options = request.Options with { PreviousStaging = [new(row.Entity, row.SourceId, row.PayloadSha256)], StagingDatasetId = "other_dataset" } };
        Assert.That(planner.Plan(request).Status, Is.EqualTo(ResultStatus.Invalid));
    }

    [Test]
    public void Equal_ids_in_distinct_entity_namespaces_do_not_collide()
    {
        var report = Run(Request(Row("same"), Row("same", "quotes")));
        Assert.That(report.Counts.WouldImport, Is.EqualTo(2)); Assert.That(report.Rows.Select(x => x.SourceKeySha256).Distinct().Count(), Is.EqualTo(2));
    }

    [Test]
    public void Complete_reference_cycles_are_bounded_and_missing_members_propagate_review()
    {
        var quote = Row("q", "quotes") with { References = [new("quote_versions", "v")] };
        var version = Row("v", "quote_versions") with { References = [new("quotes", "q")] };
        Assert.That(Run(Request(quote, version)).Counts.WouldImport, Is.EqualTo(2));
        quote = quote with { References = [new("quote_versions", "v"), new("counterparties", "missing")] };
        Assert.That(Run(Request(quote, version)).Counts.NeedsReview, Is.EqualTo(2));
    }

    [Test]
    public void Every_input_row_has_exactly_one_accounted_outcome()
    {
        var mapped = Row("mapped") with { ExternalIdentityKeys = [IdentityHash] }; var replay = Row("replay");
        var request = Request(Row("candidate"), mapped, replay, Row("review") with { Roles = [] }, Row("excluded") with { Roles = ["SUPPLIER"] }, Row("invalid") with { Issues = ["truncated_json"] });
        request = request with
        {
            Options = request.Options with
            {
                ExactIdentityMappings = new Dictionary<string, string> { { IdentityHash, Guid.NewGuid().ToString() } },
                PreviousStaging = [new(replay.Entity, replay.SourceId, replay.PayloadSha256)],
                StagingDatasetId = request.Snapshot.DatasetId
            }
        };
        Assert.That(Run(request).Counts, Is.EqualTo(new CrmHistoryDryRunCounts(6, 1, 1, 1, 1, 1, 1)));
    }

    [Test]
    public void Cancellation_is_observed_before_planning()
        => Assert.Throws<OperationCanceledException>(() => planner.Plan(Request(Row()), new CancellationToken(true)));

    [Test]
    public void Malformed_input_returns_only_fixed_safe_validation_errors()
    {
        var request = Request(Row()); request = request with { Snapshot = request.Snapshot with { DatasetId = "private@example.test" } };
        var result = planner.Plan(request);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(JsonSerializer.Serialize(result.ValidationErrors), Does.Not.Contain("private@example.test"));
    }
    [Test]
    public void Public_source_key_hash_uses_entity_and_null_separator_without_raw_id()
    {
        var row = Row(); var report = Run(Request(row));
        var bytes = System.Text.Encoding.UTF8.GetBytes(string.Concat(row.Entity, (char)0, row.SourceId));
        Assert.That(report.Rows[0].SourceKeySha256, Is.EqualTo(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))));
    }

    [TestCase("owner")]
    [TestCase("graph")]
    [TestCase("invalid")]
    public void An_old_staging_receipt_never_masks_changed_validation_or_graph_gates(string failure)
    {
        var row = Row(); var request = Request(row);
        if (failure == "owner") request = request with { Options = request.Options with { OwnerMappings = new Dictionary<string, int>() } };
        if (failure == "graph") row = row with { References = [new("quotes", "missing")] };
        if (failure == "invalid") row = row with { Issues = ["truncated_json"] };
        request = request with
        {
            Snapshot = request.Snapshot with { Rows = [row] },
            Options = request.Options with
            {
                PreviousStaging = [new(row.Entity, row.SourceId, row.PayloadSha256)],
                StagingDatasetId = request.Snapshot.DatasetId
            }
        };
        var report = Run(request);
        Assert.That(report.Status, Is.EqualTo("blocked")); Assert.That(report.Counts.AlreadyStaged, Is.Zero);
        Assert.That(failure == "invalid" ? report.Counts.Invalid : report.Counts.NeedsReview, Is.EqualTo(1));
    }

    [Test]
    public void Changed_current_payload_never_overwrites_the_previous_staging_receipt()
    {
        var row = Row(); var request = Request(row); var oldHash = Hash('3');
        request = request with { Options = request.Options with { PreviousStaging = [new(row.Entity, row.SourceId, oldHash)], StagingDatasetId = request.Snapshot.DatasetId } };
        var report = Run(request);
        Assert.That(report.StagingLedger, Has.Count.EqualTo(1)); Assert.That(report.StagingLedger[0].PayloadSha256, Is.EqualTo(oldHash));
        Assert.That(report.Counts.NeedsReview, Is.EqualTo(1));
    }

    [Test]
    public void Source_and_approval_gates_stay_blocked_even_when_every_row_was_staged_before()
    {
        var row = Row(); var request = Request(row);
        request = request with
        {
            Snapshot = request.Snapshot with { FullBackup = false },
            Options = request.Options with
            {
                PreviousStaging = [new(row.Entity, row.SourceId, row.PayloadSha256)],
                StagingDatasetId = request.Snapshot.DatasetId,
                ScopeApprovalReference = null
            }
        };
        var report = Run(request);
        Assert.That(report.Counts.AlreadyStaged, Is.EqualTo(1)); Assert.That(report.Status, Is.EqualTo("blocked"));
        Assert.That(report.Blockers, Contains.Item("source_not_full_backup")); Assert.That(report.Blockers, Contains.Item("scope_approval_missing"));
    }

    [Test]
    public void Client_membership_does_not_approve_an_unknown_additional_counterparty_role()
    {
        var report = Run(Request(Row() with { Roles = ["CLIENT", "UNKNOWN"] }));
        Assert.That(report.Counts.NeedsReview, Is.EqualTo(1)); Assert.That(report.Rows[0].Reasons, Contains.Item("role_unresolved"));
    }

    [TestCase("FINANCE")]
    [TestCase("UNKNOWN")]
    public void Guessed_or_unknown_counterparty_role_aliases_are_not_treated_as_known_roles(string role)
        => Assert.That(Run(Request(Row() with { Roles = [role] })).Counts.NeedsReview, Is.EqualTo(1));

    [Test]
    public void A_customer_identity_map_never_resolves_an_unreviewed_introduction_target_reference()
    {
        var row = Row("intro", "introduction_saved_templates") with { Issues = ["legacy_target_reference_requires_mapping"], ExternalIdentityKeys = [IdentityHash] };
        var request = Request(row); request = request with { Options = request.Options with { ExactIdentityMappings = new Dictionary<string, string> { { IdentityHash, Guid.NewGuid().ToString() } } } };
        var report = Run(request);
        Assert.That(report.Counts.NeedsReview, Is.EqualTo(1)); Assert.That(report.Rows[0].Reasons, Contains.Item("legacy_target_reference_requires_mapping"));
    }

    [Test]
    public void Deep_dependency_chains_are_iterative_and_propagate_a_missing_graph_member()
    {
        var rows = Enumerable.Range(0, 10_000).Select(i => Row("q" + i, "quotes") with
        {
            References = i == 0 ? [new CrmHistoryReference("counterparties", "missing")] : [new CrmHistoryReference("quotes", "q" + (i - 1))]
        }).ToArray();
        var report = Run(Request(rows));
        Assert.That(report.Counts.NeedsReview, Is.EqualTo(10_000)); Assert.That(report.Counts.WouldImport, Is.Zero);
    }

}

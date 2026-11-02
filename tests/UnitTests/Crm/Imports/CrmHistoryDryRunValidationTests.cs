using Ardalis.Result;
using Crm.Application.Imports.History.Contracts;
using Crm.Application.Imports.History.Planning;
using Crm.Application.Imports.History.Validation;
using static UnitTests.Crm.Imports.CrmHistoryPlannerFixture;
namespace UnitTests.Crm.Imports;

public sealed class CrmHistoryDryRunValidationTests
{
    [TestCase("owner_id")]
    [TestCase("owner_key")]
    [TestCase("target_id")]
    [TestCase("control_hash")]
    [TestCase("unknown_entity")]
    [TestCase("null_row")]
    [TestCase("null_snapshot")]
    [TestCase("too_many_rows")]
    public void Malformed_or_unbounded_operator_input_is_rejected_before_planning(string variant)
    {
        var request = Request(Row());
        request = variant switch
        {
            "owner_id" => request with { Options = request.Options with { OwnerMappings = new Dictionary<string, int> { { Owner, 0 } } } },
            "owner_key" => request with { Options = request.Options with { OwnerMappings = new Dictionary<string, int> { { "private@example.test", 17 } } } },
            "target_id" => request with { Options = request.Options with { ExactIdentityMappings = new Dictionary<string, string> { { IdentityHash, Guid.Empty.ToString() } } } },
            "control_hash" => request with { Snapshot = request.Snapshot with { ControlSha256 = "not-a-hash" } },
            "unknown_entity" => request with { Snapshot = request.Snapshot with { Rows = [Row("safe", "users")] } },
            "null_row" => request with { Snapshot = request.Snapshot with { Rows = [null!] } },
            "null_snapshot" => request with { Snapshot = null! },
            _ => Request(Enumerable.Repeat(Row(), 100_001).ToArray())
        };
        Assert.That(new CrmHistoryDryRunValidator().Validate(request).IsValid, Is.False);
        var result = new CrmHistoryDryRunPlanner().Plan(request);
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Invalid));
        Assert.That(result.ValidationErrors.All(x => x.ErrorMessage == "history_dry_run_invalid_shape"), Is.True);
    }
}

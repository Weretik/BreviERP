using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.CustomEvaluation;
using Catalog.Domain.Pricing.Sewing;
using Moq;

namespace UnitTests.Crm.CustomSewing;

public sealed class CustomSewingEvaluationTests
{
    private static readonly DateTimeOffset At = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);
    private readonly Mock<ISewingPricingSettingsReader> _settings = new();
    private readonly Mock<IPricingReferenceVersionReader> _references = new();
    private static SewingCustomPricingInput Input => new(
        [new(1, "Fabric:1", 1, "Meters", true, false, true, true)],
        [new(1, 2, 1, null, true)], new(5, 5, 12, 30));
    private static SewingPricingSettingsSelection Settings => new(10, 1, At.AddDays(-1), "sewing-sheet-v1",
        new(1000, 10, 100, 10, .25m, .1m, .1m, .1m, .1m, 10000, 2, 1.5m, 1.25m));
    private static PricingReferenceValue Fabric => new("Fabric:1", 101, 100, "Meters", true, true, "synthetic");
    private static PricingReferenceValue Operation => new("Operation:2", 102, 10, "Minutes", true, true, "synthetic");

    [SetUp]
    public void SetUp()
    {
        _settings.Reset();
        _references.Reset();
        _settings.Setup(x => x.ReadCandidatesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Settings]);
        SetReferences(Fabric, Operation);
    }

    private void SetReferences(params PricingReferenceValue[] values) =>
        _references.Setup(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<string>>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>())).ReturnsAsync(values);

    private ValueTask<Ardalis.Result.Result<SewingCustomPricePreview>> Evaluate(SewingCustomPricingInput? input = null,
        CancellationToken cancellationToken = default) =>
        new SewingCustomPricingEvaluator(_settings.Object, _references.Object)
            .EvaluateAsync(input ?? Input, At, cancellationToken);

    [Test]
    public async Task Resolves_current_inputs_and_preserves_exact_versioned_basis()
    {
        var result = await Evaluate();
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Calculation.Prices, Is.EqualTo(new SewingPriceTiers(310, 240, 205)));
            Assert.That(result.Value.Calculation.InternalCost, Is.EqualTo(152));
            Assert.That(result.Value.Settings!.Id, Is.EqualTo(10));
            Assert.That(result.Value.ReferenceVersions.Select(x => x.VersionId), Is.EquivalentTo(new[] { 101L, 102L }));
            Assert.That(result.Value.EvaluatedAt, Is.EqualTo(At));
            Assert.That(result.Value.Input.Materials, Is.EqualTo(Input.Materials));
            Assert.That(result.Value.Input.Operations, Is.EqualTo(Input.Operations));
            Assert.That(result.Value.Input.Charges, Is.EqualTo(Input.Charges));
        });
    }

    [Test]
    public async Task Later_reference_cost_changes_do_not_mutate_an_earlier_preview()
    {
        var first = (await Evaluate()).Value;
        SetReferences(Fabric with { VersionId = 103, Value = 200 }, Operation);
        var second = (await Evaluate()).Value;
        Assert.Multiple(() =>
        {
            Assert.That(first.Calculation.Prices!.Retail, Is.EqualTo(310));
            Assert.That(second.Calculation.Prices!.Retail, Is.EqualTo(510));
            Assert.That(first.ReferenceVersions.Single(x => x.ReferenceKey == "Fabric:1").VersionId, Is.EqualTo(101));
            Assert.That(second.ReferenceVersions.Single(x => x.ReferenceKey == "Fabric:1").VersionId, Is.EqualTo(103));
        });
    }

    [TestCase("missing")]
    [TestCase("ambiguous")]
    [TestCase("formula")]
    [TestCase("future")]
    public async Task Missing_ambiguous_unsupported_or_future_settings_block_without_fallback(string kind)
    {
        SewingPricingSettingsSelection[] candidates = kind switch
        {
            "missing" => [],
            "ambiguous" => [Settings, Settings with { Id = 11 }],
            "formula" => [Settings with { FormulaVersion = "unknown" }],
            _ => [Settings with { EffectiveFrom = At.AddHours(1) }]
        };
        _settings.Setup(x => x.ReadCandidatesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidates);
        var result = await Evaluate();
        var code = kind == "ambiguous" ? "AmbiguousSettings" : kind == "formula" ? "UnsupportedFormula" : "MissingSettings";
        Assert.That(result.Value.Calculation.Blockers.Select(x => x.Code), Does.Contain(code));
        Assert.That(result.Value.Calculation.Prices, Is.Null);
    }

    [Test]
    public async Task Ambiguous_reference_costs_are_not_arbitrarily_selected()
    {
        SetReferences(Fabric, Fabric with { VersionId = 103 }, Operation);
        var result = await Evaluate();
        Assert.That(result.Value.Calculation.Blockers.Select(x => x.Code), Does.Contain("AmbiguousReferenceVersion"));
        Assert.That(result.Value.Calculation.Prices, Is.Null);
    }

    [Test]
    public async Task Missing_operation_norm_does_not_get_a_legacy_id_shortcut()
    {
        SetReferences(Fabric);
        var result = await Evaluate();
        Assert.That(result.Value.Calculation.Blockers.Select(x => x.Code), Does.Contain("MissingOperationTime"));
        Assert.That(result.Value.Calculation.Prices, Is.Null);
    }

    [TestCase("Fabric:invalid")]
    [TestCase("Fabric:2147483648")]
    [TestCase("Accessory:1")]
    public async Task Malformed_or_mismatched_reference_keys_are_rejected_before_reading(string key)
    {
        var input = Input with { Materials = [Input.Materials[0] with { ReferenceKey = key }] };
        var result = await Evaluate(input);
        Assert.That(result.IsSuccess, Is.False);
        _references.Verify(x => x.ReadAsync(It.IsAny<IReadOnlyCollection<string>>(),
            It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Unapproved_or_incompatible_units_cannot_produce_a_price()
    {
        SetReferences(Fabric with { Unit = "Kilograms", IsApproved = false }, Operation);
        var result = await Evaluate();
        Assert.That(result.Value.Calculation.Blockers.Select(x => x.Code),
            Does.Contain("UnitMismatch").And.Contain("UnapprovedReference"));
        Assert.That(result.Value.Calculation.Prices, Is.Null);
    }

    [Test]
    public async Task Cancellation_is_forwarded_to_both_read_ports()
    {
        using var source = new CancellationTokenSource();
        await Evaluate(cancellationToken: source.Token);
        _settings.Verify(x => x.ReadCandidatesAsync(At, source.Token), Times.Once);
        _references.Verify(x => x.ReadAsync(It.Is<IReadOnlyCollection<string>>(keys =>
            keys.Count == 2 && keys.Contains("Fabric:1") && keys.Contains("Operation:2")), At, source.Token), Times.Once);
    }
}

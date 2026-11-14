using Ardalis.Result;
using Catalog.Domain.Pricing.Ppe;
using Catalog.Domain.Pricing.Sewing;
using Moq;
using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Commands.SyncPricingSettings;

namespace UnitTests.Catalog.Pricing.Sewing;

/// <summary>«Параметри ціноутворення» feed Catalog sewing settings and PPE rule versions (REF-004 TS-04).</summary>
public sealed class PricingSettingsSyncTests
{
    private static readonly SewingCalculationSettings Current =
        new(17500m, 20.5m, 480m, 12m, 0.25m, 0.2174m, 0.11m, 0.12m, 0.08m, 210000m, 1.55m, 1.32m, 1.25m);

    private static Dictionary<string, decimal> Sheet(SewingCalculationSettings s) => new()
    {
        ["SALARY_SEAMSTRESS"] = s.MonthlySalary, ["WORK_DAYS_MONTH"] = s.WorkDays, ["MINUTES_SHIFT"] = s.ShiftMinutes,
        ["SEAMSTRESS_COUNT"] = s.SeamstressCount, ["ASSEMBLY_RATE"] = s.AssemblyRate, ["CUTTING_RATE"] = s.CuttingRate,
        ["MASTER_RATE"] = s.MasterRate, ["SHOP_MANAGER_RATE"] = s.ManagerRate, ["SEAMSTRESS_BONUS"] = s.BonusRate,
        ["MONTHLY_OVERHEAD"] = s.MonthlyOverhead, ["SEWING_PRICE_COEF_1"] = s.RetailMultiplier,
        ["SEWING_PRICE_COEF_2"] = s.WholesaleMultiplier, ["SEWING_PRICE_COEF_3"] = s.LargeWholesaleMultiplier,
        ["PPE_PRICE_COEF_1"] = 1.4m,
    };

    private Mock<ISewingPricingSettingsReader> sewingReader = new();
    private Mock<ISewingPricingSettingsWriter> sewingWriter = new();
    private Mock<IPpePricingInputReader> ppeReader = new();
    private Mock<IPpeRuleVersionWriter> ppeWriter = new();

    [SetUp]
    public void ResetMocks()
    {
        (sewingReader, sewingWriter, ppeReader, ppeWriter) = (new(), new(), new(), new());
    }

    private SyncPricingSettingsHandler Handler()
    {
        sewingReader.Setup(x => x.ReadCandidatesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SewingPricingSettingsSelection(4, 4, DateTimeOffset.UtcNow.AddDays(-1), "v1", Current)]);
        sewingWriter.Setup(x => x.AppendAsync(It.IsAny<long>(), It.IsAny<Guid>(), It.IsAny<SewingCalculationSettings>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new SewingPricingSettingsSelection(5, 5, DateTimeOffset.UtcNow, "v1", Current)));
        ppeWriter.Setup(x => x.AppendAsync(It.IsAny<Guid?>(), It.IsAny<Guid>(), It.IsAny<PpeRuleValues>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid? _, Guid _, PpeRuleValues values, string _, CancellationToken _) =>
                Result.Success(new PpeRuleVersion(Guid.NewGuid(), values, true, "t", null, DateTimeOffset.UtcNow)));
        return new(sewingReader.Object, sewingWriter.Object, ppeReader.Object, ppeWriter.Object, TimeProvider.System);
    }

    [Test]
    public async Task A_changed_salary_appends_a_sewing_version_on_the_current_one()
    {
        var handler = Handler();
        var changed = Current with { MonthlySalary = 18000m };

        var result = (await handler.Handle(new SyncPricingSettingsCommand(Sheet(changed), [], "t"),
            CancellationToken.None)).Value;

        Assert.That(result.SewingUpdated, Is.True);
        sewingWriter.Verify(x => x.AppendAsync(4, It.IsAny<Guid>(), changed, "t", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task With_no_sewing_version_yet_the_sync_creates_the_first_one()
    {
        var handler = Handler();
        sewingReader.Setup(x => x.ReadCandidatesAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = (await handler.Handle(new SyncPricingSettingsCommand(Sheet(Current), [], "t"),
            CancellationToken.None)).Value;

        Assert.That((result.SewingUpdated, result.Skipped.Count), Is.EqualTo((true, 0)));
        sewingWriter.Verify(x => x.AppendAsync(0, It.IsAny<Guid>(), Current, "t", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Unchanged_values_append_nothing_and_missing_codes_are_reported()
    {
        var handler = Handler();
        var rule = new PpeMarkups(0.45m, 0.37m, 0.3m);
        ppeReader.Setup(x => x.ReadRuleAsync("Взуття", It.IsAny<CancellationToken>())).ReturnsAsync(
            new PpeRuleVersion(Guid.NewGuid(), new PpeRuleValues("Взуття", rule, "s", "2026-08-30"), true, "t", null,
                DateTimeOffset.UtcNow));

        var same = (await handler.Handle(new SyncPricingSettingsCommand(Sheet(Current),
            [new PpeMarkupRuleInput("Взуття", rule)], "t"), CancellationToken.None)).Value;
        var partial = Sheet(Current);
        partial.Remove("MINUTES_SHIFT");
        var missing = (await handler.Handle(new SyncPricingSettingsCommand(partial, [], "t"), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That((same.SewingUpdated, same.PpeRulesUpdated.Count, same.Skipped.Count), Is.EqualTo((false, 0, 0)));
            Assert.That(missing.Skipped.Single(), Does.Contain("MINUTES_SHIFT"));
        });
        sewingWriter.VerifyNoOtherCalls();
        ppeWriter.VerifyNoOtherCalls();
    }

    [Test]
    public async Task A_new_or_changed_ppe_rule_gets_a_version_with_the_page_as_source()
    {
        var handler = Handler();
        var current = new PpeRuleVersion(Guid.NewGuid(),
            new PpeRuleValues("Взуття", new PpeMarkups(0.4m, 0.37m, 0.3m), "s", "2026-08-30"), true, "t", null,
            DateTimeOffset.UtcNow);
        ppeReader.Setup(x => x.ReadRuleAsync("Взуття", It.IsAny<CancellationToken>())).ReturnsAsync(current);

        var result = (await handler.Handle(new SyncPricingSettingsCommand(Sheet(Current),
            [
                new PpeMarkupRuleInput("Взуття", new PpeMarkups(0.45m, 0.37m, 0.3m)),
                new PpeMarkupRuleInput("Рукавички", new PpeMarkups(0.36m, 0.36m, 0.31m)),
            ], "t"), CancellationToken.None)).Value;

        Assert.That(result.PpeRulesUpdated, Is.EqualTo(new[] { "Взуття", "Рукавички" }));
        ppeWriter.Verify(x => x.AppendAsync(current.Id, It.IsAny<Guid>(),
            It.Is<PpeRuleValues>(v => v.Code == "Взуття" && v.Source == SyncPricingSettingsHandler.Source),
            "t", It.IsAny<CancellationToken>()), Times.Once);
        ppeWriter.Verify(x => x.AppendAsync(null, It.IsAny<Guid>(), It.Is<PpeRuleValues>(v => v.Code == "Рукавички"),
            "t", It.IsAny<CancellationToken>()), Times.Once);
    }
}

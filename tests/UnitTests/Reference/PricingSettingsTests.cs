using BuildingBlocks.Domain.Results;
using Moq;
using Reference.Domain.PpeMarkupRules;
using Reference.Domain.PricingParameters;
using Reference.Application.Abstractions;
using Reference.Application.Features.PricingParameter.Queries.GetPricingParameters;
using Reference.Application.Features.PricingSettingsImport.Commands.ImportPricingSettings;
using Reference.Application.Features.PricingSettingsImport.Shared;
using Reference.Application.Features.SheetImport.Shared;

namespace UnitTests.Reference;

/// <summary>«Параметри ціноутворення»: the sheet «Налаштування цін» in Brevi (REF-004).</summary>
public sealed class PricingSettingsTests
{
    private static readonly DateOnly SheetDate = new(2026, 8, 30);
    private Mock<IPricingSettingsChangeNotifier> notifier = new();

    [SetUp]
    public void ResetNotifier() => notifier = new();

    private static PricingParameterImportRow Parameter(int row, string code, decimal? value, string unit = "%",
        bool calculated = false) =>
        new(row, code, "Виробництво", $"Параметр {code}", value, unit, calculated, SheetDate, null);

    private static PricingParameter Stored(int id, string code, decimal? value, bool calculated = false) =>
        PricingParameter.Create(PricingParameterId.From(id), code,
            new PricingParameterDetails("Виробництво", $"Параметр {code}", value, "%", calculated, null, id), SheetDate).Value;

    private static DomainResult<PricingParameter> Attempt(int id, string code, decimal? value, bool calculated = false) =>
        PricingParameter.Create(PricingParameterId.From(id), code,
            new PricingParameterDetails("Виробництво", $"Параметр {code}", value, "%", calculated, null, id), SheetDate);

    private static Mock<IReferenceRepository<T>> Repository<T>(List<T> stored, List<T> added, List<T> deleted)
        where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot
    {
        var repository = new Mock<IReferenceRepository<T>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        repository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<T>, CancellationToken>((rows, _) => added.AddRange(rows))
            .ReturnsAsync((IEnumerable<T> rows, CancellationToken _) => rows);
        repository.Setup(x => x.DeleteRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<T>, CancellationToken>((rows, _) => deleted.AddRange(rows))
            .ReturnsAsync(0);
        return repository;
    }

    [Test]
    public void Calculated_parameter_cannot_hold_a_typed_value_and_needs_a_known_formula()
    {
        Assert.Multiple(() =>
        {
            Assert.That(Attempt(1, "MINUTE_COST_SEAMSTRESS", 1.78m, calculated: true).IsFailure, Is.True);
            Assert.That(Attempt(1, "SOMETHING_ELSE", null, calculated: true).IsFailure, Is.True);
            Assert.That(Attempt(1, "ASSEMBLY_RATE", null).IsFailure, Is.True);
            Assert.That(Attempt(1, "ASSEMBLY_RATE", -1m).IsFailure, Is.True);
            Assert.That(Attempt(1, "bad code", 1m).IsFailure, Is.True);
            Assert.That(Stored(1, "assembly_rate", 0.25m).Code, Is.EqualTo("ASSEMBLY_RATE"));
        });
    }

    [Test]
    public async Task List_calculates_the_minute_cost_from_salary_days_and_shift()
    {
        var repository = new Mock<IReferenceReadRepository<PricingParameter>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(
        [
            Stored(4, "MINUTE_COST_SEAMSTRESS", null, calculated: true),
            Stored(1, "SALARY_SEAMSTRESS", 17_500m),
            Stored(2, "WORK_DAYS_MONTH", 20.5m),
            Stored(3, "MINUTES_SHIFT", 480m),
        ]);

        var rows = (await new GetPricingParametersHandler(repository.Object)
            .Handle(new GetPricingParametersQuery(), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(rows.Select(x => x.Code), Is.EqualTo(new[]
                { "SALARY_SEAMSTRESS", "WORK_DAYS_MONTH", "MINUTES_SHIFT", "MINUTE_COST_SEAMSTRESS" }));
            Assert.That(rows[3].Value, Is.EqualTo(17_500m / (20.5m * 480m)));
        });
    }

    [Test]
    public async Task Import_updates_by_code_creates_new_rows_and_deletes_old_ones()
    {
        var assembly = Stored(5, "ASSEMBLY_RATE", 0.2m);
        var legacy = PricingParameter.Create(PricingParameterId.From(9), "WORK_DAY",
            new PricingParameterDetails("Старі довідники", "кол-во раб дней", 20.5m, "шт.", false, null, 9), null).Value;
        var oldRule = PpeMarkupRule.Create(PpeMarkupRuleId.From(1), "Взуття",
            new PpeMarkupRuleDetails(0.45m, 0.37m, 0.3m, null, "Усе робоче взуття", 2)).Value;
        var (addedParameters, deletedParameters, addedRules, deletedRules) = (new List<PricingParameter>(),
            new List<PricingParameter>(), new List<PpeMarkupRule>(), new List<PpeMarkupRule>());
        var parameters = Repository([assembly, legacy], addedParameters, deletedParameters);
        var rules = Repository([oldRule], addedRules, deletedRules);
        var handler = new ImportPricingSettingsHandler(parameters.Object, rules.Object, notifier.Object);

        var result = (await handler.Handle(new ImportPricingSettingsCommand(
            [
                Parameter(6, "MINUTE_COST_SEAMSTRESS", null, "грн/хв", calculated: true),
                Parameter(7, "ASSEMBLY_RATE", 0.25m),
            ],
            [new PpeMarkupRuleImportRow(3, "Рукавички", 0.36m, 0.36m, 0.31m, "Оптова ціна діє від 10 од.", null)],
            DryRun: false, DeleteMissing: true), CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(result.Applied, Is.True);
            Assert.That((result.Parameters.Created, result.Parameters.Updated, result.Parameters.Errors),
                Is.EqualTo((1, 1, 0)));
            Assert.That(result.Parameters.Rows[1].Changes, Is.EquivalentTo(new[] { "Значення", "Порядок" }));
            Assert.That(assembly.Value, Is.EqualTo(0.25m));
            Assert.That(addedParameters.Single().Id, Is.EqualTo(PricingParameterId.From(10)));
            Assert.That(addedParameters.Single().IsCalculated, Is.True);
            Assert.That(deletedParameters, Is.EqualTo(new[] { legacy }));
            Assert.That(result.Parameters.Deleted, Is.EqualTo(new[] { "WORK_DAY кол-во раб дней" }));
            Assert.That(addedRules.Single().Name, Is.EqualTo("Рукавички"));
            Assert.That(deletedRules, Is.EqualTo(new[] { oldRule }));
        });
        parameters.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        notifier.Verify(x => x.PricingSettingsChangedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Preview_and_rows_with_errors_change_nothing()
    {
        var assembly = Stored(5, "ASSEMBLY_RATE", 0.2m);
        var parameters = Repository([assembly], [], []);
        var rules = Repository(new List<PpeMarkupRule>(), [], []);
        var handler = new ImportPricingSettingsHandler(parameters.Object, rules.Object, notifier.Object);
        var rows = new[] { Parameter(7, "ASSEMBLY_RATE", 0.25m) };

        var preview = (await handler.Handle(new ImportPricingSettingsCommand(rows, [], DryRun: true, DeleteMissing: true),
            CancellationToken.None)).Value;
        var broken = (await handler.Handle(new ImportPricingSettingsCommand(
            [.. rows, Parameter(8, "CUTTING_RATE", null)], [], DryRun: false, DeleteMissing: true),
            CancellationToken.None)).Value;

        Assert.Multiple(() =>
        {
            Assert.That(preview.Applied, Is.False);
            Assert.That(preview.Parameters.Updated, Is.EqualTo(1));
            Assert.That(broken.Applied, Is.False);
            Assert.That(broken.Parameters.Rows[1].Action, Is.EqualTo(ReferenceImportActions.Error));
            Assert.That(assembly.Value, Is.EqualTo(0.2m));
        });
        parameters.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        notifier.Verify(x => x.PricingSettingsChangedAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}

using Accounting.Application.Categories;
using Accounting.Application.History;
using Accounting.Application.Settings;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Ardalis.Result;

using static UnitTests.Accounting.FinanceMovementFixture;

namespace UnitTests.Accounting;

/// <summary>«Категорії»: two levels per direction and area, unique names among siblings, delete with children (ACC-001).</summary>
public sealed class FinanceCategoryTests
{
    [Test]
    public async Task A_subcategory_takes_the_direction_and_area_of_its_root()
    {
        using var harness = new AccountingTestHarness();
        var root = await Create(harness, "Матеріали");

        var child = await Create(harness, "Тканини", parentId: root.Id);
        var wrong = await Handler(harness).Handle(Command("Зарплата", CategoryDirection.Income, LedgerScope.Current, root.Id),
            default);
        AssertRuleBroken(await Handler(harness).Handle(
            Command("Бавовна", CategoryDirection.Expense, LedgerScope.Current, child.Id), default), "only two levels");

        Assert.Multiple(() =>
        {
            Assert.That(child.ParentId, Is.EqualTo(root.Id));
            Assert.That(child.Direction, Is.EqualTo(CategoryDirection.Expense));
            Assert.That(wrong.Status, Is.EqualTo(ResultStatus.Invalid));
        });
    }

    [Test]
    public async Task Names_are_unique_among_siblings_ignoring_case()
    {
        using var harness = new AccountingTestHarness();
        var root = await Create(harness, "Матеріали");

        var duplicate = await Handler(harness).Handle(
            Command("матеріали", CategoryDirection.Expense, LedgerScope.Current, null), default);
        var otherDirection = await Handler(harness).Handle(
            Command("Матеріали", CategoryDirection.Income, LedgerScope.Current, null), default);
        var archive = await Handler(harness).Handle(
            Command("Матеріали", CategoryDirection.Expense, LedgerScope.LegacyArchive, null), default);
        var child = await Create(harness, "Матеріали", parentId: root.Id);

        Assert.Multiple(() =>
        {
            Assert.That(duplicate.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(otherDirection.IsSuccess, Is.True);
            Assert.That(archive.IsSuccess, Is.True);
            Assert.That(child.ParentId, Is.EqualTo(root.Id), "a child may repeat its root's name");
        });
    }

    [Test]
    public async Task Rename_checks_the_version_and_duplicates()
    {
        using var harness = new AccountingTestHarness();
        var root = await Create(harness, "Матеріали");
        await Create(harness, "Оренда");
        var handler = new UpdateFinanceCategoryCommandHandler(harness.Db, harness.Time);

        var duplicate = await handler.Handle(new UpdateFinanceCategoryCommand(root.Id, "оренда", null, 0, root.Version,
            AccountingTestHarness.Director), default);
        var renamed = await handler.Handle(new UpdateFinanceCategoryCommand(root.Id, "Сировина", null, 0, root.Version,
            AccountingTestHarness.Director), default);
        var stale = await handler.Handle(new UpdateFinanceCategoryCommand(root.Id, "Інше", null, 0, root.Version,
            AccountingTestHarness.Director), default);

        Assert.Multiple(() =>
        {
            Assert.That(duplicate.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(renamed.Value.Name, Is.EqualTo("Сировина"));
            Assert.That(renamed.Value.Version, Is.EqualTo(2));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
        });
    }

    [Test]
    public async Task Deleting_a_root_deletes_its_subcategories_with_a_log_entry_each()
    {
        using var harness = new AccountingTestHarness();
        var root = await Create(harness, "Матеріали");
        var child = await Create(harness, "Тканини", parentId: root.Id);
        await Create(harness, "Оренда");

        var deleted = await new DeleteFinanceCategoryCommandHandler(harness.Db, harness.Time)
            .Handle(new DeleteFinanceCategoryCommand(root.Id, root.Version, AccountingTestHarness.Director), default);

        var left = await new GetFinanceCategoriesQueryHandler(harness.Fresh())
            .Handle(new GetFinanceCategoriesQuery(LedgerScope.Current), default);
        var childHistory = await new GetFinanceHistoryQueryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Category, child.Id), default);
        Assert.Multiple(() =>
        {
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(left.Value.Select(x => x.Name), Is.EqualTo(new[] { "Оренда" }));
            Assert.That(childHistory.Value.Select(x => x.Action), Is.EqualTo(new[] { AuditAction.Delete, AuditAction.Create }));
        });
    }

    [Test]
    public async Task Categories_are_filtered_by_area()
    {
        using var harness = new AccountingTestHarness();
        await Create(harness, "Матеріали");
        await Handler(harness).Handle(Command("Старі витрати", CategoryDirection.Expense, LedgerScope.LegacyArchive, null),
            default);

        var current = await new GetFinanceCategoriesQueryHandler(harness.Fresh())
            .Handle(new GetFinanceCategoriesQuery(LedgerScope.Current), default);
        var all = await new GetFinanceCategoriesQueryHandler(harness.Fresh())
            .Handle(new GetFinanceCategoriesQuery(null), default);

        Assert.Multiple(() =>
        {
            Assert.That(current.Value.Select(x => x.Name), Is.EqualTo(new[] { "Матеріали" }));
            Assert.That(all.Value, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task Settings_give_the_cutover_date_and_today_in_Kyiv()
    {
        using var harness = new AccountingTestHarness();
        harness.Time.SetUtcNow(new DateTimeOffset(2026, 11, 5, 22, 30, 0, TimeSpan.Zero));

        var settings = await new GetAccountingSettingsQueryHandler(harness.Time).Handle(new GetAccountingSettingsQuery(), default);

        Assert.Multiple(() =>
        {
            Assert.That(settings.Value.CutoverDate, Is.EqualTo(new DateOnly(2026, 10, 1)));
            Assert.That(settings.Value.Today, Is.EqualTo(new DateOnly(2026, 11, 6)), "00:30 in Kyiv is already the next day");
            Assert.That(LedgerCalendar.ScopeOf(new DateOnly(2026, 9, 30)), Is.EqualTo(LedgerScope.LegacyArchive));
            Assert.That(LedgerCalendar.ScopeOf(new DateOnly(2026, 10, 1)), Is.EqualTo(LedgerScope.Current));
        });
    }

    private static CreateFinanceCategoryCommandHandler Handler(AccountingTestHarness harness) => new(harness.Db, harness.Time);

    private static CreateFinanceCategoryCommand Command(string name, CategoryDirection direction, LedgerScope scope,
        Guid? parentId) => new(name, direction, scope, parentId, null, 0, AccountingTestHarness.Director);

    private static async Task<global::Accounting.Application.Contracts.FinanceCategoryResponse> Create(
        AccountingTestHarness harness, string name, Guid? parentId = null)
    {
        var result = await Handler(harness).Handle(
            Command(name, CategoryDirection.Expense, LedgerScope.Current, parentId), default);
        Assert.That(result.IsSuccess, Is.True, string.Join("; ", result.Errors.Concat(result.ValidationErrors.Select(x => x.ErrorMessage))));
        return result.Value;
    }
}

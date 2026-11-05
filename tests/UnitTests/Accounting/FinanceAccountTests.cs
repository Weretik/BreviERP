using Accounting.Application.Accounts;
using Accounting.Application.History;
using Accounting.Domain.Audit;
using Ardalis.Result;
using BuildingBlocks.Domain.Exceptions;

namespace UnitTests.Accounting;

/// <summary>«Рахунки і каси»: create, edit, cutover balance, archive and delete with the change log (ACC-001).</summary>
public sealed class FinanceAccountTests
{
    [Test]
    public async Task Create_cleans_the_fields_and_writes_a_log_entry()
    {
        using var harness = new AccountingTestHarness();

        var result = await new CreateFinanceAccountCommandHandler(harness.Db, harness.Time)
            .Handle(new CreateFinanceAccountCommand(AccountingTestHarness.Fields(name: "  ПриватБанк   ФОП "),
                AccountingTestHarness.Director), default);

        var history = await new GetFinanceHistoryQueryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Account, result.Value.Id), default);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Name, Is.EqualTo("ПриватБанк ФОП"));
            Assert.That(result.Value.Currency, Is.EqualTo("UAH"));
            Assert.That(result.Value.Iban, Is.EqualTo("UA213052990000026007000000001"));
            Assert.That(result.Value.Version, Is.EqualTo(1));
            Assert.That(result.Value.Active, Is.True);
            Assert.That(result.Value.CutoverConfirmed, Is.False);
            Assert.That(history.Value.Select(x => x.Action), Is.EqualTo(new[] { AuditAction.Create }));
            Assert.That(history.Value[0].By, Is.EqualTo(AccountingTestHarness.Director));
            Assert.That(history.Value[0].BeforeJson, Is.Null);
            Assert.That(history.Value[0].AfterJson, Does.Contain("ПриватБанк ФОП"));
        });
    }

    [Test]
    public void Domain_rejects_a_bad_currency()
    {
        Assert.Throws<DomainException>(() =>
            (AccountingTestHarness.Fields() with { Currency = "гривня" }).ToDetails());
    }

    [Test]
    public async Task Update_bumps_the_version_and_a_stale_version_is_a_conflict()
    {
        using var harness = new AccountingTestHarness();
        var created = await Create(harness);
        var handler = new UpdateFinanceAccountCommandHandler(harness.Db, harness.Time);

        var updated = await handler.Handle(new UpdateFinanceAccountCommand(created.Id,
            AccountingTestHarness.Fields(name: "Каса"), created.Version, AccountingTestHarness.Director), default);
        var stale = await handler.Handle(new UpdateFinanceAccountCommand(created.Id,
            AccountingTestHarness.Fields(name: "Інша назва"), created.Version, AccountingTestHarness.Director), default);
        var same = await handler.Handle(new UpdateFinanceAccountCommand(created.Id,
            AccountingTestHarness.Fields(name: "Каса"), updated.Value.Version, AccountingTestHarness.Director), default);

        Assert.Multiple(() =>
        {
            Assert.That(updated.Value.Name, Is.EqualTo("Каса"));
            Assert.That(updated.Value.Version, Is.EqualTo(2));
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(same.Value.Version, Is.EqualTo(2), "nothing changed, so no new version");
        });
        Assert.That(await Actions(harness, created.Id), Is.EqualTo(new[] { AuditAction.Update, AuditAction.Create }));
    }

    [Test]
    public async Task Cutover_balance_is_confirmed_with_who_and_when_and_can_be_reset()
    {
        using var harness = new AccountingTestHarness();
        var created = await Create(harness);
        var handler = new SetCutoverBalanceCommandHandler(harness.Db, harness.Time);

        var confirmed = await handler.Handle(new SetCutoverBalanceCommand(created.Id, 6_254_640, true, created.Version,
            AccountingTestHarness.Director), default);
        var reset = await handler.Handle(new SetCutoverBalanceCommand(created.Id, null, false, confirmed.Value.Version,
            AccountingTestHarness.Director), default);

        Assert.Multiple(() =>
        {
            Assert.That(confirmed.Value.CutoverBalanceMinor, Is.EqualTo(6_254_640));
            Assert.That(confirmed.Value.CutoverConfirmed, Is.True);
            Assert.That(confirmed.Value.CutoverConfirmedBy, Is.EqualTo(AccountingTestHarness.Director));
            Assert.That(confirmed.Value.CutoverConfirmedAt, Is.EqualTo(harness.Time.GetUtcNow()));
            Assert.That(reset.Value.CutoverConfirmed, Is.False);
            Assert.That(reset.Value.CutoverConfirmedBy, Is.Null);
        });
        Assert.That(await Actions(harness, created.Id),
            Is.EqualTo(new[] { AuditAction.CutoverReset, AuditAction.CutoverConfirm, AuditAction.Create }));
    }

    [Test]
    public void Confirmed_cutover_needs_a_balance()
    {
        var errors = new SetCutoverBalanceCommandValidator()
            .Validate(new SetCutoverBalanceCommand(Guid.NewGuid(), null, true, 1, AccountingTestHarness.Director));
        Assert.That(errors.Errors.Select(x => x.PropertyName), Does.Contain("BalanceMinor"));
    }

    [Test]
    public async Task Archived_accounts_are_hidden_until_asked_and_restore_brings_them_back()
    {
        using var harness = new AccountingTestHarness();
        var bank = await Create(harness, "ПриватБанк");
        await Create(harness, "Каса", sortOrder: 20);
        var active = new SetFinanceAccountActiveCommandHandler(harness.Db, harness.Time);

        var archived = await active.Handle(new SetFinanceAccountActiveCommand(bank.Id, false, bank.Version,
            AccountingTestHarness.Director), default);
        var visible = await new GetFinanceAccountsQueryHandler(harness.Fresh()).Handle(new GetFinanceAccountsQuery(false), default);
        var all = await new GetFinanceAccountsQueryHandler(harness.Fresh()).Handle(new GetFinanceAccountsQuery(true), default);
        var restored = await active.Handle(new SetFinanceAccountActiveCommand(bank.Id, true, archived.Value.Version,
            AccountingTestHarness.Director), default);

        Assert.Multiple(() =>
        {
            Assert.That(visible.Value.Select(x => x.Name), Is.EqualTo(new[] { "Каса" }));
            Assert.That(all.Value.Select(x => x.Name), Is.EqualTo(new[] { "Каса", "ПриватБанк" }), "archived go last");
            Assert.That(restored.Value.Active, Is.True);
        });
        Assert.That(await Actions(harness, bank.Id),
            Is.EqualTo(new[] { AuditAction.Restore, AuditAction.Archive, AuditAction.Create }));
    }

    [Test]
    public async Task Delete_removes_the_account_and_keeps_its_last_snapshot_in_the_log()
    {
        using var harness = new AccountingTestHarness();
        var created = await Create(harness);
        var handler = new DeleteFinanceAccountCommandHandler(harness.Db, harness.Time);

        var stale = await handler.Handle(new DeleteFinanceAccountCommand(created.Id, 7, AccountingTestHarness.Director), default);
        var deleted = await handler.Handle(new DeleteFinanceAccountCommand(created.Id, created.Version,
            AccountingTestHarness.Director), default);
        var missing = await handler.Handle(new DeleteFinanceAccountCommand(created.Id, created.Version,
            AccountingTestHarness.Director), default);

        var history = await new GetFinanceHistoryQueryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Account, created.Id), default);
        Assert.Multiple(() =>
        {
            Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
            Assert.That(deleted.IsSuccess, Is.True);
            Assert.That(missing.Status, Is.EqualTo(ResultStatus.NotFound));
            Assert.That(harness.Fresh().Accounts.Count(), Is.Zero);
            Assert.That(history.Value[0].Action, Is.EqualTo(AuditAction.Delete));
            Assert.That(history.Value[0].BeforeJson, Does.Contain("ПриватБанк ФОП"));
            Assert.That(history.Value[0].AfterJson, Is.Null);
        });
    }

    private static async Task<global::Accounting.Application.Contracts.FinanceAccountResponse> Create(
        AccountingTestHarness harness, string name = "ПриватБанк ФОП", int sortOrder = 10)
    {
        var result = await new CreateFinanceAccountCommandHandler(harness.Db, harness.Time)
            .Handle(new CreateFinanceAccountCommand(AccountingTestHarness.Fields(name: name, sortOrder: sortOrder),
                AccountingTestHarness.Director), default);
        return result.Value;
    }

    private static async Task<AuditAction[]> Actions(AccountingTestHarness harness, Guid id) =>
        (await new GetFinanceHistoryQueryHandler(harness.Fresh())
            .Handle(new GetFinanceHistoryQuery(AuditEntity.Account, id), default)).Value.Select(x => x.Action).ToArray();
}

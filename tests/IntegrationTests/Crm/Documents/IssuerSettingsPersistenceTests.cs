using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Contracts;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Documents;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntegrationTests.Crm.Documents;

[NonParallelizable]
public sealed class IssuerSettingsPersistenceTests : CrmDatabaseFixture
{
    private static readonly CrmActor Admin = new(7, true);
    private static readonly CrmActor Manager = new(8, false);
    private static IssuerSettingsInput Issuer => new("ПП «Бреві»", "35521636", "355216316015",
        "UA63 305299 00000 26004041206862", "36002, м. Полтава, вул. Кагамлика, буд. 3А",
        "+38 095 164 58 55", "pp.brevi@gmail.com", "https://brevi.com.ua/",
        null, null, null);
    private static DocumentTemplateInput Template => new("standard", "КОМЕРЦІЙНА ПРОПОЗИЦІЯ",
        "Усі ціни вказані в грн з ПДВ.", "Підготували пропозицію.", "Потрібно скоригувати бюджет?",
        "Підготуємо альтернативу.", "Зв’яжіться з {managerName}.");

    [Test]
    public async Task Admin_creates_immutable_pair_and_activates_it_with_revision_guard()
    {
        await using var db = Context();
        var store = new IssuerSettingsStore(db);
        var absent = await store.GetActiveAsync(default);
        Assert.Multiple(() => {
            Assert.That(absent.Revision, Is.EqualTo(1));
            Assert.That(absent.Active, Is.Null);
        });
        var request = Guid.NewGuid();
        var created = await store.CreateVersionAsync(request, Issuer, Template, Admin, default);
        Assert.That(created.IsSuccess, Is.True, string.Join(",", created.Errors));
        Assert.That(created.Value.IsActive, Is.False);
        var replay = await store.CreateVersionAsync(request, Issuer, Template, Admin, default);
        Assert.That(replay.Value.IssuerVersionId, Is.EqualTo(created.Value.IssuerVersionId));
        Assert.That((await store.CreateVersionAsync(request, Issuer with { LegalName = "Other" }, Template, Admin, default)).Status,
            Is.EqualTo(ResultStatus.Conflict));
        Assert.That((await store.CreateVersionAsync(Guid.NewGuid(), Issuer, Template, Manager, default)).Status,
            Is.EqualTo(ResultStatus.Forbidden));
        var activate = await store.ActivateAsync(Guid.NewGuid(), 1, created.Value.IssuerVersionId,
            created.Value.TemplateVersionId, Admin, default);
        Assert.That(activate.IsSuccess, Is.True);
        Assert.That(activate.Value.Revision, Is.EqualTo(2));
        Assert.That(activate.Value.Active!.Issuer.LegalName, Is.EqualTo(Issuer.LegalName));
        Assert.That((await store.ActivateAsync(Guid.NewGuid(), 1, created.Value.IssuerVersionId,
            created.Value.TemplateVersionId, Admin, default)).Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task Database_guards_issuer_and_template_history()
    {
        await using var db = Context();
        var created = (await new IssuerSettingsStore(db).CreateVersionAsync(Guid.NewGuid(), Issuer, Template, Admin, default)).Value;
        var change = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE crm.\"IssuerSettingsVersions\" SET \"LegalName\" = 'Other' WHERE \"PublicId\" = {created.IssuerVersionId}"));
        Assert.That(change!.SqlState, Is.EqualTo("55000"));
        var deletion = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM crm.\"DocumentTemplateVersions\" WHERE \"PublicId\" = {created.TemplateVersionId}"));
        Assert.That(deletion!.SqlState, Is.EqualTo("55000"));
    }

    [Test]
    public async Task Number_allocator_starts_at_ten_and_reserves_in_the_quote_transaction()
    {
        await using var db = Context();
        var allocator = new DocumentNumberAllocator(db);
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            Assert.That(await allocator.AllocateAsync(new DateTimeOffset(2026, 10, 28, 23, 0, 0, TimeSpan.Zero), default),
                Is.EqualTo("10/10/2026"));
            await transaction.CommitAsync();
        }
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            Assert.That(await allocator.AllocateAsync(new DateTimeOffset(2026, 10, 29, 0, 0, 0, TimeSpan.Zero), default),
                Is.EqualTo("11/10/2026"));
            await transaction.CommitAsync();
        }
    }

    [Test]
    public async Task Number_allocator_skips_an_imported_legacy_number_in_the_same_month()
    {
        await using var db = Context();
        var publicId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO crm."Quotes" ("PublicId", "RecipientName", "OwnerId", "CreatedBy", "Revision",
                "CurrentVersionNumber", "State", "Archived", "Number", "CreatedAt", "UpdatedAt")
            VALUES ({publicId}, 'Legacy', 7, 7, 1, 0, 'draft', false, '12-10-2026', now(), now())
            """);

        await using var transaction = await db.Database.BeginTransactionAsync();
        var number = await new DocumentNumberAllocator(db).AllocateAsync(
            new DateTimeOffset(2026, 10, 29, 0, 0, 0, TimeSpan.Zero), default);
        Assert.That(number, Is.EqualTo("13/10/2026"));
        await transaction.CommitAsync();
    }

    [Test]
    public async Task Concurrent_ready_transactions_receive_distinct_numbers()
    {
        var readyAt = new DateTimeOffset(2026, 10, 29, 0, 0, 0, TimeSpan.Zero);
        var firstAllocated = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<string> First()
        {
            await using var db = Context();
            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var number = await new DocumentNumberAllocator(db).AllocateAsync(readyAt, default);
                firstAllocated.TrySetResult(true);
                await Task.Delay(150);
                await transaction.CommitAsync();
                return number;
            }
            finally { firstAllocated.TrySetResult(true); }
        }

        async Task<string> Second()
        {
            await firstAllocated.Task;
            await using var db = Context();
            await using var transaction = await db.Database.BeginTransactionAsync();
            var number = await new DocumentNumberAllocator(db).AllocateAsync(readyAt, default);
            await transaction.CommitAsync();
            return number;
        }

        var numbers = await Task.WhenAll(First(), Second());
        Assert.That(numbers, Is.EquivalentTo(new[] { "10/10/2026", "11/10/2026" }));
    }
}

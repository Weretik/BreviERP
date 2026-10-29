using System.Text;
using Ardalis.Result;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Issues;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntegrationTests.Crm.Documents;

[NonParallelizable]
public sealed class QuotePdfArtifactPersistenceTests : CrmDatabaseFixture
{
    [Test]
    public async Task Queue_is_idempotent_and_successful_private_bytes_keep_their_hash()
    {
        await using var db = Context();
        var issue = await CreateIssueAsync(db);
        var store = new QuotePdfArtifactStore(db);
        var first = await store.QueueAsync(issue.QuoteId, issue.QuoteVersionId,
            issue.IssuerVersionId, issue.TemplateVersionId, "standard-v1", default);
        Assert.That(first.IsSuccess, Is.True, string.Join(",", first.Errors));
        var replay = await store.QueueAsync(issue.QuoteId, issue.QuoteVersionId,
            issue.IssuerVersionId, issue.TemplateVersionId, "standard-v1", default);
        Assert.That(replay.Value.PublicId, Is.EqualTo(first.Value.PublicId));
        Assert.That((await store.ReadContentAsync(first.Value.PublicId, default)).Status,
            Is.EqualTo(ResultStatus.Conflict));

        var claim = await store.ClaimNextAsync(default);
        Assert.That(claim, Is.Not.Null);
        Assert.That(claim!.Artifact.PublicId, Is.EqualTo(first.Value.PublicId));
        var bytes = Encoding.ASCII.GetBytes("%PDF-1.7\n%%EOF");
        var completed = await store.CompleteAsync(first.Value.PublicId, claim.LeaseToken, bytes, default);
        Assert.That(completed.IsSuccess, Is.True, string.Join(",", completed.Errors));
        Assert.That(completed.Value.Status, Is.EqualTo("succeeded"));
        var read = await store.ReadContentAsync(first.Value.PublicId, default);
        Assert.That(read.IsSuccess, Is.True, string.Join(",", read.Errors));
        Assert.That(read.Value.Bytes, Is.EqualTo(bytes));
        Assert.That(read.Value.Artifact.Sha256, Is.EqualTo(completed.Value.Sha256));
        Assert.That((await store.RetryAsync(first.Value.PublicId, default)).IsSuccess, Is.False);
        Assert.That((await store.CompleteAsync(first.Value.PublicId, claim.LeaseToken, bytes, default)).IsSuccess, Is.False);
        var update = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE crm.\"QuotePdfArtifacts\" SET \"Sha256\" = 'changed' WHERE \"PublicId\" = {first.Value.PublicId}"));
        Assert.That(update!.SqlState, Is.EqualTo("55000"));
        var delete = Assert.ThrowsAsync<PostgresException>(async () => await db.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM crm.\"QuotePdfArtifacts\" WHERE \"PublicId\" = {first.Value.PublicId}"));
        Assert.That(delete!.SqlState, Is.EqualTo("55000"));
    }

    [Test]
    public async Task Historical_pdf_bytes_and_hash_survive_a_new_current_quote_version()
    {
        await using var db = Context();
        var issue = await CreateIssueAsync(db);
        var store = new QuotePdfArtifactStore(db);
        var queued = await store.QueueAsync(issue.QuoteId, issue.QuoteVersionId,
            issue.IssuerVersionId, issue.TemplateVersionId, "standard-v1", default);
        Assert.That(queued.IsSuccess, Is.True);
        var claim = await store.ClaimNextAsync(default);
        Assert.That(claim, Is.Not.Null);
        var originalBytes = Encoding.ASCII.GetBytes("%PDF-1.7\nhistorical %%EOF");
        var saved = await store.CompleteAsync(queued.Value.PublicId, claim!.LeaseToken, originalBytes, default);
        Assert.That(saved.IsSuccess, Is.True);
        var originalHash = saved.Value.Sha256;

        var quote = await db.Quotes.SingleAsync(x => x.Id == issue.QuoteId);
        var next = QuoteVersion.Create(quote.Id, quote.CurrentVersionNumber + 1,
            issue.QuoteVersionId, "edited", quote.RecipientName, null, null, 7, []);
        db.QuoteVersions.Add(next);
        await db.SaveChangesAsync();
        quote.AppendVersion(quote.Revision, next.Id, 7);
        await db.SaveChangesAsync();
        Assert.That(quote.CurrentVersionId, Is.EqualTo(next.Id));

        var historical = await store.ReadContentAsync(queued.Value.PublicId, default);
        Assert.That(historical.IsSuccess, Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(historical.Value.Bytes, Is.EqualTo(originalBytes));
            Assert.That(historical.Value.Artifact.Sha256, Is.EqualTo(originalHash));
            Assert.That(historical.Value.Artifact.QuoteVersionId, Is.EqualTo(issue.QuoteVersionId));
        });
    }

    [Test]
    public async Task Artifact_fk_rejects_a_pinned_pair_different_from_quote_issue()
    {
        await using var db = Context();
        var issue = await CreateIssueAsync(db);
        var other = DocumentTemplateVersion.Create(2, "standard", "Інший шаблон",
            "Усі ціни вказані в грн з ПДВ.", "Вступ", "Зв'язок", "Деталі", "Контакт", new CrmUserId(7));
        db.DocumentTemplateVersions.Add(other);
        await db.SaveChangesAsync();
        var store = new QuotePdfArtifactStore(db);
        Assert.ThrowsAsync<DbUpdateException>(async () => await store.QueueAsync(issue.QuoteId,
            issue.QuoteVersionId, issue.IssuerVersionId, other.Id, "wrong-template", default));
    }

    private async Task<QuoteIssue> CreateIssueAsync(CrmDbContext db)
    {
        var actor = new CrmUserId(7);
        var quote = Quote.Create(null, null, "Одержувач", actor);
        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        var version = QuoteVersion.Create(quote.Id, 1, null, "created", "Одержувач",
            null, null, actor.Value, []);
        db.QuoteVersions.Add(version);
        await db.SaveChangesAsync();
        quote.SetInitialVersion(version.Id);
        var issuer = IssuerSettingsVersion.Create(1, "Бреві", "12345678", "1234567890",
            "UA123456789012345678901234567", "Київ", "+380501234567", "office@example.com",
            "https://brevi.example", null, null, null, actor);
        var template = DocumentTemplateVersion.Create(1, "standard", "Комерційна пропозиція",
            "Усі ціни вказані в грн з ПДВ.", "Вступ", "Зв'язок", "Деталі", "Контакт", actor);
        db.IssuerSettingsVersions.Add(issuer);
        db.DocumentTemplateVersions.Add(template);
        await db.SaveChangesAsync();
        var issue = QuoteIssue.Create(quote.Id, version.Id, issuer.Id, template.Id,
            "10/10/2026", DateTimeOffset.UtcNow);
        db.Set<QuoteIssue>().Add(issue);
        await db.SaveChangesAsync();
        return issue;
    }
}

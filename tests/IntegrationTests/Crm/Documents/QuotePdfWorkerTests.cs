using System.Text;
using System.Text.Json;
using Crm.Application.Abstractions.Documents;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.Source;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Issues;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.Pricing;
using Crm.Domain.Quotes.Variants;
using Crm.Domain.Quotes.Versions;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Crm.Infrastructure.Persistence.Documents.Reading;
using Host.Api.Crm.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Crm.Documents;

[NonParallelizable]
public sealed class QuotePdfWorkerTests : CrmDatabaseFixture
{
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7\n%%EOF");

    [TestCase(true, "succeeded")]
    [TestCase(false, "failed")]
    public async Task Hosted_worker_processes_a_queued_pinned_artifact(
        bool rendererAvailable, string expectedStatus)
    {
        Guid artifactId;
        await using (var db = Context())
        {
            var issue = await CreateIssueAsync(db);
            var queued = await new QuotePdfArtifactStore(db).QueueAsync(issue.QuoteId,
                issue.QuoteVersionId, issue.IssuerVersionId, issue.TemplateVersionId,
                "standard-v1", default);
            Assert.That(queued.IsSuccess, Is.True, string.Join(",", queued.Errors));
            artifactId = queued.Value.PublicId;
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(Connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<IQuotePdfArtifactStore, QuotePdfArtifactStore>();
        services.AddScoped<IQuoteDocumentSource, QuoteDocumentSource>();
        var renderer = new RecordingRenderer();
        if (rendererAvailable) services.AddSingleton<IPrivateDocumentRenderer>(renderer);
        services.AddSingleton<QuotePdfWorker>();
        await using var provider = services.BuildServiceProvider();
        var worker = provider.GetRequiredService<QuotePdfWorker>();
        await worker.StartAsync(default);
        try
        {
            QuotePdfArtifactMetadata? metadata = null;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            while (!timeout.IsCancellationRequested)
            {
                await using var scope = provider.CreateAsyncScope();
                metadata = await scope.ServiceProvider.GetRequiredService<IQuotePdfArtifactStore>()
                    .FindAsync(artifactId, default);
                if (metadata?.Status == expectedStatus) break;
                await Task.Delay(100, timeout.Token);
            }
            Assert.That(metadata?.Status, Is.EqualTo(expectedStatus));
            if (rendererAvailable)
            {
                Assert.That(renderer.Document?.Title, Does.Contain("10/10/2026"));
                await using var scope = provider.CreateAsyncScope();
                var content = await scope.ServiceProvider.GetRequiredService<IQuotePdfArtifactStore>()
                    .ReadContentAsync(artifactId, default);
                Assert.That(content.IsSuccess, Is.True, string.Join(",", content.Errors));
                Assert.That(content.Value.Bytes, Is.EqualTo(Pdf));
                Assert.That(content.Value.Artifact.Sha256, Is.Not.Empty);
            }
            else
            {
                Assert.That(metadata?.ErrorCode, Is.EqualTo("renderer_unavailable"));
                Assert.That(renderer.Document, Is.Null);
            }
        }
        finally { await worker.StopAsync(default); }
    }

    private static async Task<QuoteIssue> CreateIssueAsync(CrmDbContext db)
    {
        var actor = new CrmUserId(7);
        var quote = Quote.Create(null, null, "Одержувач", actor);
        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
        var basis = new QuotePricePreview(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(5),
            new("oneOffPpe", null, null, null, new(100, 90, 80, "Synthetic")), null,
            100, 90, 80, [], "UAH", "manual_no_extra_vat");
        var basisJson = JsonSerializer.Serialize(basis, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var item = QuoteItem.Create(Guid.NewGuid(), 0, "Offer", null, null, "Private", true,
            [QuoteVariant.Create(Guid.NewGuid(), 0, "Variant", "oneOffPpe", null, null,
                true, basis.BasisToken, basisJson,
                [QuoteTier.Create("retail", 100, true), QuoteTier.Create("wholesale", 90, false),
                    QuoteTier.Create("largeWholesale", 80, false)])]);
        var version = QuoteVersion.Create(quote.Id, 1, null, "created", "Одержувач",
            null, null, actor.Value, [item]);
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

    private sealed class RecordingRenderer : IPrivateDocumentRenderer
    {
        public PublicDocumentProjection? Document { get; private set; }
        public Task<byte[]> RenderAsync(PublicDocumentProjection document, CancellationToken cancellationToken)
        {
            Document = document;
            return Task.FromResult(Pdf);
        }
    }
}

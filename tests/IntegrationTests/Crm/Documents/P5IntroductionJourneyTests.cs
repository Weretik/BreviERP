using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Crm.Application.Abstractions.Documents;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.Contracts;
using Crm.Application.Documents.Projection;
using Crm.Application.Documents.RenderQuotePdf;
using Crm.Application.Documents.Source;
using Crm.Application.Quotes.BuildIntroduction;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.Introductions;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.PrepareQuote;
using Crm.Application.Quotes.Pricing;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes.Introductions;
using Crm.Infrastructure.Persistence;
using Crm.Infrastructure.Persistence.Documents;
using Crm.Infrastructure.Persistence.Documents.Artifacts;
using Crm.Infrastructure.Persistence.Documents.Reading;
using Crm.Infrastructure.Persistence.Introductions;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Host.Api.Crm.Documents;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IntegrationTests.Crm.Documents;

/// <summary>SC-I02/03 and SC-D01/02 persisted pipeline; real Chromium has separate acceptance.</summary>
[NonParallelizable]
public sealed class P5IntroductionJourneyTests : CrmDatabaseFixture
{
    private static readonly CrmActor Manager = new(17, false);
    private static readonly CrmActor Admin = new(7, true);

    [TestCase(IntroductionLayout.General, 2, false)]
    [TestCase(IntroductionLayout.Summer, 1, false)]
    [TestCase(IntroductionLayout.Winter, 1, false)]
    [TestCase(IntroductionLayout.Summer, 1, true,
        TestName = "Oversized_introduction_layout_blocks_ready_without_number_or_issue")]
    public async Task Saved_selection_ready_artifact_and_reopen_keep_history_after_current_price_changes(
        IntroductionLayout layout, int expectedPages, bool oversizedHeader)
    {
        await using var db = Context();
        var settings = new IssuerSettingsStore(db);
        var configured = await settings.CreateVersionAsync(Guid.NewGuid(),
            new IssuerSettingsInput("Synthetic Seller", "12345678", "1234567890",
                "UA123456789012345678901234567", "Kyiv", "+380501234567",
                "seller@example.com", "https://brevi.com.ua", null, null, null),
            new DocumentTemplateInput("standard", "Комерційна пропозиція", "Усі ціни вказані в грн з ПДВ.",
                oversizedHeader ? new string('x', 1001) : "Дякуємо за інтерес",
                "Зв'язок", "Замовте зразок", "Ваш менеджер {managerName}"), Admin, default);
        Assert.That(configured.IsSuccess, Is.True);
        Assert.That((await settings.ActivateAsync(Guid.NewGuid(), 1, configured.Value.IssuerVersionId,
            configured.Value.TemplateVersionId, Admin, default)).IsSuccess, Is.True);

        var mappingStore = new IntroductionFeaturedMappingStore(db);
        var mapping = await mappingStore.ActivateAsync(IntroductionFeaturedMappingVersion.Create(new string('a', 64),
            "MOD-SYNTHETIC-SUMMER", "MOD-SYNTHETIC-WINTER", 100, 104, new CrmUserId(7)), default);
        var required = IntroductionSlot.Required(layout);
        var selections = required.Select(slot => new IntroductionSelection(slot,
            (slot.Season == IntroductionSeason.Summer ? 100 : 104) + slot.Position, 200 + slot.Uid)).ToArray();
        var templates = new IntroductionTemplateStore(db);
        var saved = await templates.CreateAsync(Manager, Guid.NewGuid(), "Збережена синтетична добірка",
            new IntroductionTemplatePayload(mapping, layout, Guid.NewGuid(), selections), default);
        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.Errors));

        var currentPrice = 200m;
        var currentSource = Guid.NewGuid();
        QuoteCatalogPrice Current(int product, long? article) => new("sewing", product, article,
            currentSource, null, null, 1, 1, [], "PRIVATE_COST_FORMULA_SENTINEL", 9999m, 8888m, currentPrice, []);
        var prices = new Mock<IQuoteCatalogPriceReader>();
        prices.Setup(x => x.ReadAsync("sewing", It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string _, int product, long? article, CancellationToken _) => Current(product, article));
        var catalog = new Mock<IIntroductionCatalogReader>();
        catalog.Setup(x => x.ReadAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int product, long article, CancellationToken _) => new IntroductionCatalogOffer(
                product < 104 ? IntroductionSeason.Summer : IntroductionSeason.Winter, "made", true, true,
                $"Модель {product}", "Саржа", "60% бавовна, 40% поліестер", "240 г/м²",
                $"https://brevi.com.ua/product/{product}", Current(product, article)));
        var templateService = new IntroductionTemplateService(templates, mappingStore, new IntroductionDraftBuilder(catalog.Object),
            new global::Crm.Infrastructure.Persistence.Documents.EmailTemplates.EmailTemplateStore(db));
        var checkedDraft = await templateService.ApplyAsync(Manager, saved.Value.TemplatePublicId, saved.Value.VersionPublicId, default);
        Assert.That(checkedDraft.IsSuccess, Is.True, string.Join(",", checkedDraft.Errors));
        Assert.That(checkedDraft.Value.Slots.All(x => x.LargeWholesale == 200m), Is.True);

        var basisStore = new QuotePriceBasisStore(db);
        var createHandler = new CreateQuoteCommandHandler(new QuoteMutationStore(db),
            new QuoteVersionBuilder(new QuotePriceBasisResolver(basisStore, prices.Object)));
        var sender = new Mock<ISender>();
        sender.Setup(x => x.Send(It.IsAny<CreateQuoteCommand>(), It.IsAny<CancellationToken>()))
            .Returns((CreateQuoteCommand command, CancellationToken ct) => createHandler.Handle(command, ct));
        var creator = new IntroductionQuoteCreator(templateService, basisStore, sender.Object, new IntroductionQuoteReplayReader(db));
        var applicationId = Guid.NewGuid();
        var created = await creator.CreateAsync(Manager, applicationId, saved.Value.TemplatePublicId,
            saved.Value.VersionPublicId, null, null, "ТОВ Синтетичний Замовник", default);
        Assert.That(created.IsSuccess, Is.True, string.Join(",", created.Errors));
        Assert.That(created.Value.State, Is.EqualTo("draft"));
        Assert.That(created.Value.Version.Items, Has.Count.EqualTo(required.Count));

        var prepare = new PrepareQuoteCommandHandler(new QuoteMutationStore(db), new DocumentNumberAllocator(db), new NoLogo());
        var ready = await prepare.Handle(new(created.Value.PublicId, created.Value.Version.PublicId,
            created.Value.Revision, Guid.NewGuid(), Manager), default);
        if (oversizedHeader)
        {
            Assert.That(ready.Status, Is.EqualTo(ResultStatus.Invalid));
            Assert.That(ready.ValidationErrors.Select(x => x.ErrorMessage),
                Does.Contain("introduction_public_layout_too_large"));
            Assert.That(await db.QuoteIssues.CountAsync(), Is.Zero);
            Assert.That(await db.DocumentNumberSequences.CountAsync(), Is.Zero);
            var unchanged = await db.Quotes.AsNoTracking().SingleAsync();
            Assert.That(unchanged.Number, Is.Null);
            return;
        }
        Assert.That(ready.IsSuccess, Is.True, string.Join(",", ready.Errors));
        Assert.That(ready.Value.State, Is.EqualTo("ready"));
        Assert.That(ready.Value.Number, Is.Not.Empty);
        var renderCommand = new RenderQuotePdfCommand(created.Value.PublicId, created.Value.Version.PublicId, Manager);
        var artifacts = new QuotePdfArtifactStore(db);
        var queued = await new RenderQuotePdfCommandHandler(new QuoteDocumentSource(db), artifacts).Handle(renderCommand, default);
        Assert.That(queued.IsSuccess, Is.True);
        var renderer = new RecordingRenderer();
        await RunWorkerAsync(queued.Value.ArtifactId, renderer);
        var first = await artifacts.ReadContentAsync(queued.Value.ArtifactId, default);
        Assert.That(first.IsSuccess, Is.True);
        var originalHash = first.Value.Artifact.Sha256;
        Assert.That(originalHash, Is.Not.Empty);
        Assert.That(renderer.Document?.Pages, Has.Count.EqualTo(expectedPages));
        var projectionJson = JsonSerializer.Serialize(renderer.Document);
        Assert.That(projectionJson, Does.Not.Contain("PRIVATE_COST_FORMULA_SENTINEL"));
        Assert.That(renderer.Document!.Pages!.SelectMany(x => x.Products).All(x => x.LargeWholesale == 200m), Is.True);

        currentPrice = 275m;
        currentSource = Guid.NewGuid();
        var fresh = await creator.CreateAsync(Manager, Guid.NewGuid(), saved.Value.TemplatePublicId,
            saved.Value.VersionPublicId, null, null, "ТОВ Новий Замовник", default);
        Assert.That(fresh.IsSuccess, Is.True, string.Join(",", fresh.Errors));
        var freshVersion = await db.QuoteVersions.AsNoTracking().SingleAsync(x => x.PublicId == fresh.Value.Version.PublicId);
        Assert.That(freshVersion.IntroductionPublicSnapshot!.Slots.All(x => x.LargeWholesale == 275m), Is.True);
        var oldVersion = await db.QuoteVersions.AsNoTracking().SingleAsync(x => x.PublicId == created.Value.Version.PublicId);
        Assert.That(oldVersion.IntroductionPublicSnapshot!.Slots.All(x => x.LargeWholesale == 200m), Is.True);
        var issue = await db.QuoteIssues.AsNoTracking().SingleAsync();
        var source = await new QuoteDocumentSource(db).LoadRenderSourceAsync(first.Value.Artifact, default);
        Assert.That(source, Is.Not.Null);
        var reopenedProjection = QuoteDocumentProjectionSelector.Build(source!.Version, source.Issuer,
            source.Template, source.Issue.Number, source.PublicContactName);
        Assert.That(reopenedProjection.Pages!.SelectMany(x => x.Products).All(x => x.LargeWholesale == 200m), Is.True);
        Assert.That(issue.Number, Is.EqualTo(ready.Value.Number));
        var repeated = await new RenderQuotePdfCommandHandler(new QuoteDocumentSource(db), artifacts).Handle(renderCommand, default);
        Assert.That(repeated.Value.ArtifactId, Is.EqualTo(queued.Value.ArtifactId));
        var reread = await artifacts.ReadContentAsync(repeated.Value.ArtifactId, default);
        Assert.That(reread.Value.Artifact.Sha256, Is.EqualTo(originalHash));
        Assert.That(reread.Value.Bytes, Is.EqualTo(first.Value.Bytes));
        var replay = await creator.CreateAsync(Manager, applicationId, saved.Value.TemplatePublicId,
            saved.Value.VersionPublicId, null, null, "ТОВ Синтетичний Замовник", default);
        Assert.That(replay.Value.PublicId, Is.EqualTo(created.Value.PublicId));
        Assert.That(await db.Quotes.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.QuoteIssues.CountAsync(), Is.EqualTo(1));
    }

    private async Task RunWorkerAsync(Guid artifactId, RecordingRenderer renderer)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<CrmDbContext>(options => options.UseNpgsql(Connection,
            x => x.MigrationsHistoryTable("__EFMigrationsHistory", "crm")));
        services.AddScoped<IQuotePdfArtifactStore, QuotePdfArtifactStore>();
        services.AddScoped<IQuoteDocumentSource, QuoteDocumentSource>();
        services.AddSingleton<IPrivateDocumentRenderer>(renderer);
        services.AddSingleton<QuotePdfWorker>();
        await using var provider = services.BuildServiceProvider();
        var worker = provider.GetRequiredService<QuotePdfWorker>();
        await worker.StartAsync(default);
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            while (true)
            {
                await using var scope = provider.CreateAsyncScope();
                var artifact = await scope.ServiceProvider.GetRequiredService<IQuotePdfArtifactStore>().FindAsync(artifactId, default);
                if (artifact?.Status == "succeeded") break;
                Assert.That(artifact?.Status, Is.Not.EqualTo("failed"), artifact?.ErrorCode);
                await Task.Delay(50, timeout.Token);
            }
        }
        finally { await worker.StopAsync(default); }
    }

    private sealed class NoLogo : IQuoteBrandAssetResolver
    {
        public Task<PublicDocumentImage?> ResolveAsync(string assetKey, CancellationToken cancellationToken)
            => Task.FromResult<PublicDocumentImage?>(null);
    }

    private sealed class RecordingRenderer : IPrivateDocumentRenderer
    {
        public PublicDocumentProjection? Document { get; private set; }
        public Task<byte[]> RenderAsync(PublicDocumentProjection document, CancellationToken cancellationToken)
        {
            Document = document;
            return Task.FromResult(Encoding.ASCII.GetBytes("%PDF-1.7\nfixture renderer only\n%%EOF"));
        }
    }
}

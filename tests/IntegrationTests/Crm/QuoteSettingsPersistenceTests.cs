using Ardalis.Result;
using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Commands.SaveQuoteSettings;
using Crm.Application.Features.Quotes.Queries.GetQuotePdf;
using Crm.Application.Features.Quotes.Queries.GetQuoteSettings;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Pdf;
using Crm.Application.Features.Quotes.Shared.Pdf.Composing;
using Crm.Application.Features.Quotes.Shared.Settings;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.Items;
using Crm.Domain.Quotes.ValueObjects;

namespace IntegrationTests.Crm;

/// <summary>«Налаштування КП» and the PDF source over the real crm schema (CRM-003 stage 2).</summary>
[NonParallelizable]
public sealed class QuoteSettingsPersistenceTests : CrmDatabaseFixture
{
    private static readonly QuoteActor Director = new(1, true);
    private static readonly QuoteActor Olena = new(17, false);
    private readonly QuoteTestStaff _staff = new();
    private readonly QuoteTestClock _clock = new(new DateTimeOffset(2026, 11, 4, 10, 0, 0, TimeSpan.Zero));

    private async Task<QuoteSettingsResponse> Load()
    {
        await using var db = Context();
        return (await new GetQuoteSettingsHandler(db, _staff).Handle(new GetQuoteSettingsQuery(), default)).Value;
    }

    private async Task<Result<QuoteSettingsResponse>> Save(QuoteActor actor, SaveQuoteSettingsInput input)
    {
        await using var db = Context();
        return await new SaveQuoteSettingsHandler(db, _staff, _clock)
            .Handle(new SaveQuoteSettingsCommand(actor, input), default);
    }

    private static SaveQuoteSettingsInput ToInput(QuoteSettingsResponse x) =>
        new(x.Version, x.Company, x.Email, x.Pdf, x.Introduction, x.LetterVariants,
            x.StaffContacts.Select(c => new QuoteStaffContactInput(c.UserId, c.Name, c.Phone, c.Email)).ToArray());

    [Test]
    public async Task FirstSaveStoresTheDefaultsAndLaterSavesNeedTheLoadedVersion()
    {
        var defaults = await Load();
        Assert.That(defaults.Version, Is.Zero);
        Assert.That(defaults.StaffContacts.Select(x => x.Name), Is.EqualTo(new[] { "Директор", "Олена" }));

        var input = ToInput(defaults);
        input = input with
        {
            Company = input.Company with { LogoUrl = "https://cdn.brevi.test/logo.svg" },
            StaffContacts = [new QuoteStaffContactInput(17, "Олена Петренко", "+38 050 000 00 00", "olena@brevi.test")],
        };
        var saved = await Save(Director, input);
        Assert.That(saved.IsSuccess, Is.True, string.Join(",", saved.Errors));

        var reloaded = await Load();
        Assert.That(reloaded.Version, Is.EqualTo(1));
        Assert.That(reloaded.UpdatedByName, Is.EqualTo("Директор"));
        Assert.That(reloaded.Company.LogoUrl, Is.EqualTo("https://cdn.brevi.test/logo.svg"));
        Assert.That(reloaded.Introduction.Advantages, Has.Count.EqualTo(5));
        Assert.That(reloaded.StaffContacts.Single(x => x.UserId == 17).Name, Is.EqualTo("Олена Петренко"));

        var stale = await Save(Director, input);
        Assert.That(stale.Status, Is.EqualTo(ResultStatus.Conflict));
    }

    [Test]
    public async Task ManagerCannotChangeTheSettings()
    {
        var result = await Save(Olena, ToInput(await Load()));
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Forbidden));
    }

    [Test]
    public async Task PdfUsesTheResponsibleManagersContactAndHidesOtherQuotes()
    {
        var input = ToInput(await Load()) with
        {
            StaffContacts = [new QuoteStaffContactInput(17, "Олена Петренко", "+38 050 000 00 00", "olena@brevi.test")],
        };
        Assert.That((await Save(Director, input)).IsSuccess, Is.True);

        long quoteId;
        await using (var db = Context())
        {
            var option = new QuoteItemOption(0, null, null, "Ціна", null, false, new QuotePrice(100m, null, true),
                new QuotePrice(90m, null, false), new QuotePrice(80m, null, true), _clock.GetUtcNow());
            var quote = Quote.Create(new QuoteNumber(2026, 11, 1), QuoteKind.Request, new QuotePerson(17, "Олена"),
                new QuoteHeader("Внутрішнє", null, null, "ТОВ «Агро»", null, "Нотатка"),
                [new QuoteItem(0, QuoteItemKind.OneOffPpe, null, new QuoteItemTexts(null, "Каска", null, null, null, null, null, null), [option])],
                _clock.GetUtcNow()).Value;
            db.Quotes.Add(quote);
            await db.SaveChangesAsync();
            quoteId = quote.Id;
        }

        var renderer = new QuoteTestRenderer();
        await using (var db = Context())
        {
            var own = await new GetQuotePdfHandler(db, new QuotePdfComposer(db, _staff, renderer, new QuoteTestNoCatalog())).Handle(new GetQuotePdfQuery(Olena, quoteId), default);
            Assert.That(own.Value.FileName, Is.EqualTo("КП 1-11-2026.pdf"));
            var other = await new GetQuotePdfHandler(db, new QuotePdfComposer(db, _staff, renderer, new QuoteTestNoCatalog()))
                .Handle(new GetQuotePdfQuery(new QuoteActor(18, false), quoteId), default);
            Assert.That(other.Status, Is.EqualTo(ResultStatus.NotFound));
        }

        Assert.That(renderer.Model!.Manager, Is.EqualTo(new QuotePdfContact("Олена Петренко", "+38 050 000 00 00", "olena@brevi.test")));
        Assert.That(renderer.Model.Tiers, Is.EqualTo(new QuotePdfTiers(true, false, true)));
    }
}

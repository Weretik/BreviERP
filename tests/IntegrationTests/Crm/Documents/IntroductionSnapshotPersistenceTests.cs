using Crm.Domain.Customers.Identifiers;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.Versions;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Crm.Documents;

[NonParallelizable]
public sealed class IntroductionSnapshotPersistenceTests : CrmDatabaseFixture
{
    [Test]
    public async Task Issued_version_reads_the_original_public_slots_from_jsonb()
    {
        var required = IntroductionSlot.Required(IntroductionLayout.General);
        var pin = new IntroductionPublicSnapshot(IntroductionLayout.General, Guid.NewGuid(),
            Guid.NewGuid(), required.Select((slot, index) => new IntroductionPublicSlotSnapshot(
                slot, 100 + index, 200 + index, $"Модель {index}", "Тканина",
                "Бавовна", "240 г/м²", $"https://brevi.com.ua/products/{index}",
                700 + index)).ToArray());
        Guid versionPublicId;
        await using (var db = Context())
        {
            var quote = Quote.Create(null, null, "Одержувач", new CrmUserId(7));
            db.Quotes.Add(quote);
            await db.SaveChangesAsync();
            var version = QuoteVersion.Create(quote.Id, 1, null, "created", "Одержувач",
                null, null, 7, [], pin);
            db.QuoteVersions.Add(version);
            await db.SaveChangesAsync();
            versionPublicId = version.PublicId;
        }

        await using var read = Context();
        var saved = await read.QuoteVersions.AsNoTracking()
            .SingleAsync(x => x.PublicId == versionPublicId);
        Assert.Multiple(() =>
        {
            Assert.That(saved.IntroductionPublicSnapshot?.Layout, Is.EqualTo(IntroductionLayout.General));
            Assert.That(saved.IntroductionPublicSnapshot?.MappingVersionId, Is.EqualTo(pin.MappingVersionId));
            Assert.That(saved.IntroductionPublicSnapshot?.Slots, Has.Count.EqualTo(8));
            Assert.That(saved.IntroductionPublicSnapshot?.Slots[4].Slot.Season,
                Is.EqualTo(IntroductionSeason.Winter));
            Assert.That(saved.IntroductionPublicSnapshot?.Slots[4].LargeWholesale, Is.EqualTo(704));
        });
    }
}

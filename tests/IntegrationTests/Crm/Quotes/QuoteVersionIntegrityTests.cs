using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Inputs;
using Crm.Application.Quotes.CreateQuote;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Moq;
using Npgsql;

namespace IntegrationTests.Crm.Quotes;

[NonParallelizable]
public sealed class QuoteVersionIntegrityTests : CrmDatabaseFixture
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task DatabaseRejectsVersionFromAnotherQuote(bool currentPointer)
    {
        await using var db = Context();
        var handler = new CreateQuoteCommandHandler(new QuoteMutationStore(db),
            new QuoteVersionBuilder(new QuotePriceBasisResolver(
                new QuotePriceBasisStore(db), Mock.Of<IQuoteCatalogPriceReader>())));
        for (var index = 0; index < 2; index++)
        {
            var created = await handler.Handle(new(null, null, $"Synthetic {index}",
                new QuoteDraftContent([]), Guid.NewGuid(), new CrmActor(17, false)), default);
            Assert.That(created.IsSuccess, Is.True);
        }
        var quotes = await db.Quotes.OrderBy(x => x.Id).ToArrayAsync();
        var firstId = quotes[0].Id.Value;
        var firstVersion = quotes[0].CurrentVersionId!.Value;
        var foreignVersion = quotes[1].CurrentVersionId!.Value;

        var exception = Assert.ThrowsAsync<PostgresException>(async () =>
        {
            if (currentPointer)
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE crm.\"Quotes\" SET \"CurrentVersionId\" = {foreignVersion} WHERE \"Id\" = {firstId}");
            else
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE crm.\"QuoteVersions\" SET \"ParentVersionId\" = {foreignVersion} WHERE \"Id\" = {firstVersion}");
        });
        Assert.That(exception!.SqlState, Is.EqualTo(PostgresErrorCodes.ForeignKeyViolation));
    }

    [Test]
    public async Task OwnershipMigrationRollsBackAndReappliesWithoutLosingDraftOrIdGeneration()
    {
        await using var db = Context();
        var handler = new CreateQuoteCommandHandler(new QuoteMutationStore(db),
            new QuoteVersionBuilder(new QuotePriceBasisResolver(
                new QuotePriceBasisStore(db), Mock.Of<IQuoteCatalogPriceReader>())));
        var command = new CreateQuoteCommand(null, null, "Before rollback", new QuoteDraftContent([]),
            Guid.NewGuid(), new CrmActor(17, false));
        var first = await handler.Handle(command, default);
        Assert.That(first.IsSuccess, Is.True);

        await db.GetService<IMigrator>().MigrateAsync("20261026151551_AddQuoteDrafts");
        await db.Database.MigrateAsync();

        var second = await handler.Handle(command with { RequestId = Guid.NewGuid() }, default);
        Assert.That(second.IsSuccess, Is.True);
        Assert.That(await db.Quotes.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.QuoteVersions.CountAsync(), Is.EqualTo(2));
        Assert.That(await db.Quotes.AnyAsync(x => x.PublicId == first.Value.PublicId), Is.True);
    }
}

using Catalog.Infrastructure.Pricing.History.Calculations;
using Catalog.Infrastructure.Pricing.History.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace IntegrationTests.Pricing.Sewing;

[NonParallelizable]
public sealed class SewingPricingMigrationTests : SewingPricingDatabaseFixture
{
    [Test]
    public async Task History_migration_upgrades_existing_catalogue_and_uncommitted_calculations_roll_back()
    {
        await using var catalog = Catalog();
        var products = await catalog.Products.CountAsync();
        var migrator = catalog.GetService<IMigrator>();
        await migrator.MigrateAsync("20261025102032_AddSewingPricingSettingsVersions");
        await catalog.Database.MigrateAsync();
        Assert.That(await catalog.Products.CountAsync(), Is.EqualTo(products));
        Assert.That(catalog.Database.HasPendingModelChanges(), Is.False);
        var transactions = new SewingPricingTransaction(catalog);
        var calculations = new SewingCalculationStore(catalog);
        var request = Guid.NewGuid();
        var emptyJson = "{}";
        await using (await transactions.BeginAsync(1, CancellationToken.None))
        {
            await catalog.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO catalog."SewingCalculations"
                    ("PublicId", "RequestId", "ProductId", "ArticleId", "SnapshotJson", "CreatedAt", "CreatedBy")
                SELECT {Guid.NewGuid()}, {request}, a."ProductId", a."Id", {emptyJson}::jsonb, now(), 'rollback-test'
                FROM catalog."SewingArticles" a ORDER BY a."Id" LIMIT 1
                """);
        }
        Assert.That(await calculations.FindRequestAsync(request, CancellationToken.None), Is.Null);
        var otherProduct = await catalog.Products.Where(x => x.ModelCode == null).Select(x => x.Id.Value).SingleAsync();
        var mismatch = Assert.ThrowsAsync<PostgresException>(async () =>
            await catalog.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO catalog."SewingCalculations"
                    ("PublicId", "RequestId", "ProductId", "ArticleId", "SnapshotJson", "CreatedAt", "CreatedBy")
                SELECT {Guid.NewGuid()}, {Guid.NewGuid()}, {otherProduct}, a."Id", {emptyJson}::jsonb, now(), 'invalid-parent'
                FROM catalog."SewingArticles" a ORDER BY a."Id" LIMIT 1
                """));
        Assert.That(mismatch!.SqlState, Is.EqualTo(PostgresErrorCodes.ForeignKeyViolation));
    }
}

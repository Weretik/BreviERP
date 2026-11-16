using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Platform.Outbox;

/// <summary>PLT-003: a run reads the outbox table the module actually maps, in its own schema.</summary>
public sealed class OutboxTableTests
{
    [Test]
    public void A_context_with_a_default_schema_reads_the_outbox_there()
    {
        using var db = new OutboxContext(builder => builder.HasDefaultSchema("warehouse").AddOutbox());

        Assert.That(OutboxModelBuilderExtensions.QuotedTable(db.Model), Is.EqualTo("\"warehouse\".\"OutboxMessages\""));
    }

    [Test]
    public void A_context_that_maps_the_outbox_schema_by_hand_reads_it_there_not_from_public()
    {
        // As Catalog does: no default schema, the outbox table placed in "catalog".
        using var db = new OutboxContext(builder =>
            builder.AddOutbox().Entity<OutboxMessage>().ToTable(OutboxModelBuilderExtensions.Table, "catalog"));

        Assert.That(OutboxModelBuilderExtensions.QuotedTable(db.Model), Is.EqualTo("\"catalog\".\"OutboxMessages\""));
    }

    [Test]
    public void A_context_without_any_schema_reads_public()
    {
        using var db = new OutboxContext(builder => builder.AddOutbox());

        Assert.That(OutboxModelBuilderExtensions.QuotedTable(db.Model), Is.EqualTo("\"public\".\"OutboxMessages\""));
    }

    private sealed class OutboxContext(Action<ModelBuilder> configure) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
            // Only the model is read; the connection is never opened. Each mapping gets its own model cache entry.
            options.UseSqlite("Data Source=:memory:").ReplaceService<
                Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory, PerInstanceModelCacheKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) => configure(modelBuilder);
    }

    private sealed class PerInstanceModelCacheKey : Microsoft.EntityFrameworkCore.Infrastructure.IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) => (context, designTime);
    }
}

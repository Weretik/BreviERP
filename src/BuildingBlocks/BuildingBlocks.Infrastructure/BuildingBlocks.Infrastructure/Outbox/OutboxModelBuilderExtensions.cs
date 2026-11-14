using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Outbox;

public static class OutboxModelBuilderExtensions
{
    public const string Table = "OutboxMessages";

    /// <summary>The module's <c>OutboxMessages</c> table, in the schema of its <c>DbContext</c>.</summary>
    public static ModelBuilder AddOutbox(this ModelBuilder builder)
    {
        builder.Entity<OutboxMessage>(b =>
        {
            b.ToTable(Table);
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.Type).HasMaxLength(OutboxMessage.TypeMaxLength).IsRequired();
            b.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
            b.Property(x => x.LastError).HasMaxLength(OutboxMessage.ErrorMaxLength);
            b.Ignore(x => x.GaveUp);
            b.HasIndex(x => new { x.ProcessedAt, x.NextAttemptAt });
        });
        return builder;
    }
}

using Reference.Infrastructure.Pricing.InputVersions.Changes;

namespace Reference.Infrastructure.DataBase;

public partial class ReferenceDbContext
{
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (new ReferencePricingChangeAppender(this).HasChanges())
            throw new InvalidOperationException("Pricing input edits require SaveChangesAsync for atomic version capture.");
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var appender = new ReferencePricingChangeAppender(this);
        if (!appender.HasChanges())
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        var ownsTransaction = Database.CurrentTransaction is null;
        var transaction = ownsTransaction ? await Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken) : null;
        try
        {
            await Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025004)", cancellationToken);
            await appender.AppendAsync(cancellationToken);
            var affected = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            if (transaction is not null)
                await transaction.CommitAsync(cancellationToken);
            return affected;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }
}

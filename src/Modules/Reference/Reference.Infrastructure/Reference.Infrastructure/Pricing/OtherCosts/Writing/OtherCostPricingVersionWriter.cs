using System.Security.Cryptography;
using System.Text;
using Ardalis.Result;
using Reference.Application.Contracts.Pricing;
using Reference.Domain.Pricing;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Pricing.OtherCosts.Writing;

public sealed class OtherCostPricingVersionWriter(ReferenceDbContext db) : IOtherCostPricingVersionWriter
{
    public async Task<Result<long>> AppendAsync(string code, long expectedVersionId, decimal value, Guid requestId,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025004)", cancellationToken);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"other-cost:{requestId:D}")));
        var repeated = await db.Set<ReferencePricingInputVersion>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.SourceHash == hash, cancellationToken);
        if (repeated is not null)
            return repeated.OtherCostCode == code && repeated.Value == value
                ? Result.Success(repeated.Id.Value) : Result.Conflict("Request UUID was used for a different cost revision.");
        var latest = await db.Set<ReferencePricingInputVersion>().AsNoTracking()
            .Where(x => x.OtherCostCode == code).OrderByDescending(x => x.Revision).FirstOrDefaultAsync(cancellationToken);
        if (latest is null)
            return Result.NotFound();
        var at = DateTimeOffset.UtcNow;
        if (latest.Id.Value != expectedVersionId || latest.EffectiveFrom >= at)
            return Result.Conflict("Cost version changed or a future revision exists.");
        var next = ReferencePricingInputVersion.Create(null, null, null, code, value, latest.Unit,
            latest.IsActive, true, latest.Revision + 1, at, latest.SourceCode, hash, at.ToString("O"));
        db.Add(next);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(next.Id.Value);
    }
}

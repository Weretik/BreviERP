using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Records;
namespace Catalog.Infrastructure.Pricing.Ppe.Writing;
public sealed class PpeRuleVersionWriter(CatalogDbContext db) : IPpeRuleVersionWriter
{
    public async Task<Result<PpeRuleVersion>> AppendAsync(Guid? expectedVersionId, Guid requestId, PpeRuleValues values, string actor, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025006)", cancellationToken);
        var json = JsonSerializer.Serialize(values);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { expectedVersionId, values, actor }))));
        var replay = await db.Set<PpeRuleVersionRecord>().AsNoTracking().SingleOrDefaultAsync(x => x.RequestId == requestId, cancellationToken);
        if (replay is not null)
            return replay.RequestHash == hash ? Result.Success(replay.ToVersion()) : Result.Conflict("Request UUID was used with different input or actor.");

        var current = await db.Set<PpeRuleVersionRecord>().AsNoTracking().Where(x => x.Code == values.Code).OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (current?.PublicId != expectedVersionId) return Result.Conflict("Pricing input changed. Reload before saving.");
        var now = DateTimeOffset.UtcNow;
        var record = new PpeRuleVersionRecord { PublicId = Guid.NewGuid(), RequestId = requestId, RequestHash = hash,
            ValuesJson = json, ApprovedBy = actor, ApprovedAt = now, CreatedAt = now, Code = values.Code, };
        db.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(record.ToVersion());
    }
}

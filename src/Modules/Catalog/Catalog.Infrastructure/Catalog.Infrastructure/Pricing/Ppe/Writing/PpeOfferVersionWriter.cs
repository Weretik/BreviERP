using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ardalis.Result;
using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Ppe.Records;
namespace Catalog.Infrastructure.Pricing.Ppe.Writing;
public sealed class PpeOfferVersionWriter(CatalogDbContext db) : IPpeOfferVersionWriter
{
    public async Task<Result<PpeOfferVersion>> AppendAsync(int productId, Guid? expectedVersionId, Guid requestId, PpeOfferValues values, string actor, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(681025006)", cancellationToken);
        var json = JsonSerializer.Serialize(values);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { productId, expectedVersionId, values, actor }))));
        var replay = await db.Set<PpeOfferVersionRecord>().AsNoTracking().SingleOrDefaultAsync(x => x.RequestId == requestId, cancellationToken);
        if (replay is not null)
            return replay.RequestHash == hash ? Result.Success(replay.ToVersion()) : Result.Conflict("Request UUID was used with different input or actor.");
        var ids = new[] { productId };
        if (!await db.Products.FromSqlInterpolated($"SELECT * FROM catalog.\"Products\" WHERE \"Id\" = ANY({ids})")
            .AnyAsync(x => !x.IsDeleted && x.Type == Catalog.Domain.Products.Enums.ProductType.Ppe, cancellationToken))
            return Result.NotFound();
        var current = await db.Set<PpeOfferVersionRecord>().AsNoTracking().Where(x => x.ProductId == Catalog.Domain.Products.ValueObjects.ProductId.Create(productId)).OrderByDescending(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (current?.PublicId != expectedVersionId) return Result.Conflict("Pricing input changed. Reload before saving.");
        var now = DateTimeOffset.UtcNow;
        var record = new PpeOfferVersionRecord { PublicId = Guid.NewGuid(), RequestId = requestId, RequestHash = hash,
            ValuesJson = json, ApprovedBy = actor, ApprovedAt = now, CreatedAt = now, ProductId = Catalog.Domain.Products.ValueObjects.ProductId.Create(productId), };
        db.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(record.ToVersion());
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Contracts;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock;

/// <summary>
/// Repeats of a stock form by its operation id: the same person and data return the saved result, other data with the
/// same id is refused (409), so a lost answer never moves the fabric twice.
/// </summary>
public sealed class StockOperations(IWarehouseDbContext db)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>SHA-256 of the person, the kind and the request; <paramref name="request"/> leaves out version guards.</summary>
    public static string Hash(int userId, string kind, object request) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { userId, kind, request }, Json))));

    /// <summary>The saved result of this operation, a conflict when the id was used for other data, or null.</summary>
    public async Task<Result<FabricStockOperationResponse>?> ReplayAsync(Guid id, string hash,
        CancellationToken cancellationToken)
    {
        var saved = await db.StockOperations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (saved is null) return null;
        if (saved.RequestHash != hash) return Result<FabricStockOperationResponse>.Conflict(StockMessages.OperationReused);
        return Result.Success(new FabricStockOperationResponse(saved.Id, saved.FabricCode, saved.Color,
            saved.MetersAfter, true));
    }
}

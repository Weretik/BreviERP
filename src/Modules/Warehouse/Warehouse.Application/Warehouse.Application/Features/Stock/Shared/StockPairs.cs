using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;

namespace Warehouse.Application.Features.Stock.Shared;

/// <summary>
/// Loads a fabric and colour pair for a change, restates the ledgers it touched and saves it with its operation id.
/// </summary>
public sealed class StockPairs(IWarehouseDbContext db, StockOperations operations, TimeProvider clock)
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public DateTimeOffset Now => clock.GetUtcNow();

    /// <summary>Today in Kyiv, the default operation date.</summary>
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Now, Kyiv).DateTime);

    /// <summary>Every line of the alias group of <paramref name="code"/> whose colour key is that of <paramref name="color"/>.</summary>
    public async Task<StockPair> LoadAsync(string code, string color, CancellationToken cancellationToken)
    {
        var codes = FabricAliases.Members(code).ToArray();
        var key = StockColor.Key(color);
        var lines = await db.FabricStocks.Where(x => codes.Contains(x.FabricCode)).ToListAsync(cancellationToken);
        return new StockPair(code, key, lines.Where(x => StockColor.Key(x.Color) == key));
    }

    /// <summary>
    /// Adds new lines, restates the chain of every line touched by operation date, record time and id, records the
    /// operation and saves. A change by someone else in between is a conflict; a parallel repeat of the same id
    /// returns the saved result.
    /// </summary>
    public async Task<Result<FabricStockOperationResponse>> SaveAsync(WarehouseActor actor, Guid operationId,
        string kind, string hash, StockPair pair, StockPairResult result, CancellationToken cancellationToken)
    {
        foreach (var line in result.Lines.Where(x => x.Id == 0)) db.FabricStocks.Add(line);
        var ids = result.Lines.Where(x => x.Id != 0).Select(x => x.Id).ToArray();
        var saved = ids.Length == 0
            ? []
            : await db.FabricMovements.Where(x => ids.Contains(x.StockId)).ToListAsync(cancellationToken);
        foreach (var line in result.Lines) line.Restate(saved.Where(x => x.StockId == line.Id).Concat(line.Movements));

        var color = pair.Lines.FirstOrDefault()?.Color ?? result.Lines[0].Color;
        db.StockOperations.Add(new StockOperation(operationId, kind, hash, actor.UserId, pair.Code, pair.ColorKey, color,
            pair.Meters, Now));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another person changed the pair, or the same operation was saved in parallel: answer the saved result.
            if (await operations.ReplayAsync(operationId, hash, cancellationToken) is { } replay) return replay;
            return Result<FabricStockOperationResponse>.Conflict(StockMessages.Stale);
        }

        return Result.Success(new FabricStockOperationResponse(operationId, pair.Code, color, pair.Meters, false));
    }

    public static Result<FabricStockOperationResponse> Refused(StockPairRefusal refusal) => refusal switch
    {
        StockPairRefusal.AlreadyOpened => Result<FabricStockOperationResponse>.Conflict(StockMessages.AlreadyOpened),
        StockPairRefusal.NotHeld => Result<FabricStockOperationResponse>.NotFound(StockMessages.NotHeld),
        StockPairRefusal.NothingToAdjust => Invalid("TargetMeters", StockMessages.NothingToAdjust),
        StockPairRefusal.DeepensShortage => Invalid("TargetMeters", StockMessages.DeepensShortage),
        _ => Invalid("Meters", StockMessages.OutOfRange)
    };

    private static Result<FabricStockOperationResponse> Invalid(string field, string message) =>
        Result<FabricStockOperationResponse>.Invalid(new ValidationError(field, message));
}

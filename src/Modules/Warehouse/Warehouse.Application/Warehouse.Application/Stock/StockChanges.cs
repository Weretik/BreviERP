using Warehouse.Application.Abstractions;
using Warehouse.Application.Contracts;
using Warehouse.Application.Access;
using Warehouse.Application.Stock.GetStockLine;
using Warehouse.Domain.Stock;

namespace Warehouse.Application.Stock;

/// <summary>
/// Common steps of a stock change: the person, the line and its version, the fabric of «Тканини», saving with the
/// concurrency check, and the new card.
/// </summary>
public sealed class StockChanges(IWarehouseDbContext db, IWarehouseStaffReader staff, IWarehouseReferenceReader reference,
    StockReservations reservations, StockRows rows, TimeProvider clock)
{
    public DateTimeOffset Now => clock.GetUtcNow();

    public async Task<string?> ActorNameAsync(WarehouseActor actor, CancellationToken cancellationToken) =>
        (await staff.FindAsync(actor.UserId, cancellationToken))?.Name;

    /// <summary>The fabric as a stock line keeps it; an error when it is missing or has no FAB- code.</summary>
    public async Task<Result<StockFabric>> FabricAsync(int fabricId, CancellationToken cancellationToken)
    {
        var fabric = (await reference.FabricsAsync(cancellationToken)).FirstOrDefault(x => x.Id == fabricId);
        if (fabric is null) return Result<StockFabric>.NotFound(StockMessages.FabricNotFound);
        if (string.IsNullOrWhiteSpace(fabric.Code)) return Result<StockFabric>.Invalid(
            new ValidationError(nameof(fabricId), StockMessages.FabricWithoutCode));
        return Result.Success(new StockFabric(fabric.Id, fabric.Code, fabric.Name));
    }

    /// <summary>The line of a fabric and colour, created (not yet saved) when the stock never held it.</summary>
    public async Task<FabricStock> LineAsync(StockFabric fabric, string color, CancellationToken cancellationToken)
    {
        var code = fabric.Code.Trim().ToUpperInvariant();
        var key = StockColor.Key(color);
        var line = db.FabricStocks.Local.FirstOrDefault(x => x.FabricCode == code && x.ColorKey == key)
                   ?? await db.FabricStocks.SingleOrDefaultAsync(x => x.FabricCode == code && x.ColorKey == key,
                       cancellationToken);
        if (line is not null)
        {
            line.Rename(fabric);
            return line;
        }

        line = new FabricStock(fabric, color, Now);
        db.FabricStocks.Add(line);
        return line;
    }

    /// <summary>Saves; another person's change in between is 409.</summary>
    public async Task<Result> SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(StockMessages.Stale);
        }
        catch (DbUpdateException)
        {
            // Two people opened the same new fabric and colour at once: the unique index refused the second.
            return Result.Conflict(StockMessages.Stale);
        }
    }

    public async Task<FabricStockLineResponse> CardAsync(WarehouseActor actor, FabricStock line,
        CancellationToken cancellationToken) =>
        await GetFabricStockLineQueryHandler.BuildAsync(db, reservations, rows, actor, line, cancellationToken);
}

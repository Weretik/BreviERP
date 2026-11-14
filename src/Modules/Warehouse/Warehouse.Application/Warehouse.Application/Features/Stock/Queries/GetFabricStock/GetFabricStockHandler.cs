using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Queries.GetFabricStock.Results;
using Warehouse.Application.Features.Stock.Shared.Booking;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricStock;

public sealed class GetFabricStockHandler(
    IWarehouseDbContext db,
    IWarehouseReferenceReader reference,
    FabricBookingCalculator booking)
    : IQueryHandler<GetFabricStockQuery, Result<FabricStockListResponse>>
{
    public async ValueTask<Result<FabricStockListResponse>> Handle(GetFabricStockQuery query,
        CancellationToken cancellationToken)
    {
        var lines = await db.FabricStocks.AsNoTracking().ToListAsync(cancellationToken);
        var fabrics = (await reference.FabricsAsync(cancellationToken)).Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => FabricAliases.Normalize(x.Code!)).ToDictionary(g => g.Key, g => g.First());
        var booked = (await booking.CalculateAsync(cancellationToken)).Pairs
            .ToDictionary(x => (x.FabricCode, StockColor.Key(x.Color)));
        var last = await LastMovements(lines, cancellationToken);

        var result = lines.GroupBy(x => FabricAliases.Canonical(x.FabricCode)).Select(group =>
            {
                var colors = group.GroupBy(x => StockColor.Key(x.Color))
                    .Select(x => new StockPair(group.Key, x.Key, x))
                    .Select(pair => Color(pair, booked.GetValueOrDefault((group.Key, pair.ColorKey)),
                        last.GetValueOrDefault((group.Key, pair.ColorKey))))
                    .OrderBy(x => x.Color, StringComparer.CurrentCulture).ToList();
                var alias = FabricAliases.Group(group.Key);
                var fabric = fabrics.GetValueOrDefault(group.Key);
                var anchor = group.OrderBy(x => x.FabricCode == group.Key ? 0 : 1).ThenBy(x => x.Id).First();
                return new FabricStockFabricResponse(group.Key, FabricAliases.Members(group.Key),
                    alias?.Name ?? fabric?.Name ?? anchor.FabricName, alias?.Composition ?? fabric?.Composition,
                    alias?.Density ?? fabric?.Density, colors.Sum(x => x.Meters), colors,
                    string.IsNullOrWhiteSpace(fabric?.Supplier) ? null : fabric.Supplier.Trim(),
                    colors.Sum(x => x.Booked), colors.Sum(x => x.Free));
            })
            .OrderBy(x => x.FabricName, StringComparer.CurrentCulture).ThenBy(x => x.FabricCode, StringComparer.Ordinal)
            .ToList();
        var pairs = result.SelectMany(x => x.Colors).ToList();
        return Result.Success(new FabricStockListResponse(result, pairs.Sum(x => x.Meters), pairs.Count,
            pairs.Count(x => x.Meters < 0), StockList.Permissions(query.Actor), pairs.Count(x => x.ToOrder > 0)));
    }

    private static FabricStockColorResponse Color(StockPair pair, FabricBookingPairResponse? booking,
        FabricStockLastMovementResponse? last)
    {
        var need = booking?.Need ?? 0m;
        var bookings = (booking?.Items ?? []).GroupBy(x => (x.OrderId, x.ItemId))
            .Select(x => new FabricStockBookingResponse(x.Key.OrderId, x.First().OrderNumber, x.First().ClientName,
                x.First().ModelName, x.First().DueDate, x.Sum(i => i.Remaining), x.Sum(i => i.ToOrder)))
            .Where(x => x.Need > 0).ToList();
        return new FabricStockColorResponse(pair.Lines[0].Color, pair.ColorKey, pair.Meters, pair.Version,
            pair.Lines.Select(l => new FabricStockSourceResponse(l.Id, l.FabricCode, l.Meters)).ToList(), need,
            pair.Meters - need, booking?.ToOrder ?? 0m, last, bookings);
    }

    /// <summary>The latest movement of each pair that still counts, by the ledger order (operation date first).</summary>
    private async Task<Dictionary<(string Code, string Key), FabricStockLastMovementResponse>> LastMovements(
        IReadOnlyList<FabricStock> lines, CancellationToken cancellationToken)
    {
        var pairOf = lines.ToDictionary(x => x.Id, x => (FabricAliases.Canonical(x.FabricCode), StockColor.Key(x.Color)));
        var movements = await db.FabricMovements.AsNoTracking().Where(x => !x.IsVoided)
            .Select(x => new
            {
                x.Id, x.StockId, x.Kind, x.OperationDate, x.CreatedAt, x.Delta, x.OrderNumber, x.Document,
                x.SupplierName, x.Destination,
            })
            .ToListAsync(cancellationToken);
        return movements.Where(x => pairOf.ContainsKey(x.StockId)).GroupBy(x => pairOf[x.StockId])
            .ToDictionary(g => g.Key, g =>
            {
                var x = g.OrderByDescending(m => m.OperationDate).ThenByDescending(m => m.CreatedAt)
                    .ThenByDescending(m => m.Id).First();
                return new FabricStockLastMovementResponse(x.Id, x.Kind.ToString(), x.OperationDate, x.Delta,
                    x.OrderNumber, x.Document, x.SupplierName, x.Destination);
            });
    }
}

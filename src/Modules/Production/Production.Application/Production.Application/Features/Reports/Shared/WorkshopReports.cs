using System.Text.Json;
using Production.Application.Abstractions;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Abstractions.Materials;
using Production.Application.Features.Payroll.Shared.Employee;
using Production.Application.Features.Reports.Shared.Rows;

namespace Production.Application.Features.Reports.Shared;

/// <summary>
/// «Звіти цеху» (PRD-D48, D49). A month is counted from the work log, «Зарплата», the fabric movements of «Склад
/// тканин» and the prices of «Тканини»; a month closed in «Зарплата» keeps its report as it was. Several months are the sum of their months;
/// the stock is the balance at the end of the last one.
/// </summary>
public sealed class WorkshopReports(IProductionDbContext db, PayrollSheets sheets, IProductionFabricReader fabrics,
    IProductionStockReader stockReader)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static string Serialize(WorkshopReportResponse report) => JsonSerializer.Serialize(report, Json);

    /// <summary>The months <paramref name="from"/>..<paramref name="to"/> (first days), as kept or as they are now.</summary>
    public async Task<WorkshopReportResponse> ReadAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var kept = await db.PayrollMonths.AsNoTracking()
            .Where(x => x.Month >= from && x.Month <= to && x.ReportSnapshot != null)
            .ToDictionaryAsync(x => x.Month, x => x.ReportSnapshot!, cancellationToken);
        var months = new List<(WorkshopReportResponse Report, bool Closed)>();
        for (var month = from; month <= to; month = month.AddMonths(1))
            months.Add(kept.TryGetValue(month, out var json)
                ? (JsonSerializer.Deserialize<WorkshopReportResponse>(json, Json)!, true)
                : (await CalculateAsync(month, cancellationToken), false));
        return Merge(PayrollMonths.Format(from), PayrollMonths.Format(to), months);
    }

    /// <summary>One month from the current data; the payroll part follows «Зарплата» (a closed month as closed).</summary>
    public async Task<WorkshopReportResponse> CalculateAsync(DateOnly month, CancellationToken cancellationToken)
    {
        var next = month.AddMonths(1);
        var prices = (await fabrics.ListAsync(cancellationToken)).Where(x => x.Code != null)
            .GroupBy(x => x.Code!.Trim().ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First().Price is > 0 ? g.First().Price : null);
        decimal? PriceOf(string code) => prices.GetValueOrDefault(code.Trim().ToUpperInvariant());

        var output = await OutputAsync(month, next, cancellationToken);
        var payroll = (await sheets.ReadAsync(month, cancellationToken)).Month;
        var staff = payroll.Lines.Where(x => x.Accrued != 0 || x.Minutes > 0 || x.Sewn > 0 || x.Cut > 0)
            .Select(StaffRow).ToList();

        var materials = (await stockReader.IssuesAsync(month, next, cancellationToken)).Select(x =>
        {
            var price = PriceOf(x.FabricCode);
            return new ReportIssueRowResponse(x.MovementId, x.OperationDate, x.FabricCode, x.FabricName, x.Color,
                x.OrderId, x.OrderNumber, x.ModelName, x.Meters, price, price is { } p ? Money(x.Meters * p) : null);
        }).ToList();

        var stockRows = (await stockReader.BalancesAsync(next, cancellationToken))
            .Select(x =>
            {
                var price = PriceOf(x.FabricCode);
                return new ReportStockRowResponse(x.StockId, x.FabricCode, x.FabricName, x.Color, x.Meters, price,
                    price is { } p ? Money(Math.Max(0m, x.Meters) * p) : null);
            })
            .Where(x => x.Meters != 0)
            .OrderBy(x => x.FabricCode, StringComparer.Ordinal).ThenBy(x => x.Color, StringComparer.CurrentCulture)
            .ToList();

        var text = PayrollMonths.Format(month);
        return new WorkshopReportResponse(text, text, [], [text], Totals(output, staff, materials, stockRows), output,
            staff, materials, stockRows, payroll.Warnings);
    }

    private async Task<List<ReportOutputRowResponse>> OutputAsync(DateOnly month, DateOnly next,
        CancellationToken cancellationToken)
    {
        var sewn = (await db.Work.AsNoTracking()
                .Where(x => x.Kind == ProductionWorkKind.Sewing && x.VoidedAt == null && !x.OverPlan &&
                            x.WorkDate >= month && x.WorkDate < next)
                .Select(x => new { x.OrderId, x.ItemId, x.Quantity })
                .ToListAsync(cancellationToken))
            .GroupBy(x => (x.OrderId, x.ItemId))
            .ToDictionary(x => x.Key, x => x.Sum(w => w.Quantity));
        var orderIds = sewn.Keys.Select(x => x.OrderId).Distinct().ToList();
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => orderIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        return orders.SelectMany(order => order.Items
                .Where(item => sewn.ContainsKey((order.Id, item.Id)))
                .Select(item =>
                {
                    var quantity = sewn[(order.Id, item.Id)];
                    var price = item.ReportUnitPrice is > 0 ? item.ReportUnitPrice : null;
                    return new ReportOutputRowResponse(order.Id, order.Number, order.ClientName, item.Id,
                        item.ModelName, quantity, price, price is { } p ? Money(quantity * p) : null);
                }))
            .OrderBy(x => x.OrderNumber).ThenBy(x => x.ItemId)
            .ToList();
    }

    /// <summary>Sums months: output and staff by item and worker, materials one after another, stock of the last month.</summary>
    public static WorkshopReportResponse Merge(string from, string to,
        IReadOnlyList<(WorkshopReportResponse Report, bool Closed)> months)
    {
        var output = months.SelectMany(x => x.Report.Output)
            .GroupBy(x => x.ItemId)
            .Select(g =>
            {
                var first = g.First();
                var quantity = g.Sum(x => x.Quantity);
                return first with
                {
                    Quantity = quantity,
                    Value = g.All(x => x.Value is not null) ? g.Sum(x => x.Value!.Value) : null
                };
            })
            .OrderBy(x => x.OrderNumber).ThenBy(x => x.ItemId).ToList();
        var staff = months.SelectMany(x => x.Report.Staff)
            .GroupBy(x => x.EmployeeId)
            .Select(g =>
            {
                var last = g.Last();
                var minutes = g.Sum(x => x.Minutes);
                var accrued = g.Sum(x => x.Accrued);
                return last with
                {
                    Minutes = minutes, Sewn = g.Sum(x => x.Sewn), Cut = g.Sum(x => x.Cut), Accrued = accrued,
                    DayAverage = DayAverage(accrued, minutes)
                };
            })
            .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
        var materials = months.SelectMany(x => x.Report.Materials).ToList();
        var stock = months.Count > 0 ? months[^1].Report.Stock : [];
        var warnings = months.SelectMany(x => x.Report.Warnings).Distinct().ToList();
        return new WorkshopReportResponse(from, to,
            [.. months.Where(x => x.Closed).Select(x => x.Report.From)],
            [.. months.Where(x => !x.Closed).Select(x => x.Report.From)],
            Totals(output, staff, materials, stock), output, staff, materials, stock, warnings);
    }

    private static WorkshopReportTotalsResponse Totals(IReadOnlyList<ReportOutputRowResponse> output,
        IReadOnlyList<ReportStaffRowResponse> staff, IReadOnlyList<ReportIssueRowResponse> materials,
        IReadOnlyList<ReportStockRowResponse> stock) => new(
        output.Sum(x => x.Quantity),
        output.Sum(x => x.Value ?? 0),
        output.Where(x => x.Value is null).Sum(x => x.Quantity),
        staff.Sum(x => x.Accrued),
        staff.Sum(x => x.Minutes),
        materials.Sum(x => x.Meters),
        materials.Sum(x => x.Value ?? 0),
        materials.Where(x => x.Value is null).Sum(x => x.Meters),
        stock.Sum(x => x.Meters),
        stock.Sum(x => x.Value ?? 0),
        stock.Where(x => x.Value is null).Sum(x => x.Meters));

    public static ReportStaffRowResponse StaffRow(PayrollLineResponse x) => new(x.EmployeeId, x.Name, x.Position,
        x.Minutes, x.Sewn, x.Cut, x.Accrued, DayAverage(x.Accrued, x.Minutes));

    /// <summary>Pay per standard 8-hour day; nothing without hours.</summary>
    public static decimal? DayAverage(decimal accrued, int minutes) =>
        minutes > 0 ? Money(accrued * 480m / minutes) : null;

    private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}

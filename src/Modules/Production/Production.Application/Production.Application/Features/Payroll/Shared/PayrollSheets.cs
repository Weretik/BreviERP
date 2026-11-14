using System.Text.Json;
using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Domain.Staff;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.PayrollParameters.Shared;
using Production.Application.Features.Payroll.Shared.Employee;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Shared;

/// <summary>A month of «Зарплата» with the work behind it; a closed month keeps exactly this as JSON.</summary>
public sealed record PayrollSnapshot(PayrollMonthResponse Month, IReadOnlyList<PayrollWorkRowResponse> Work);

/// <summary>
/// Builds a month of «Зарплата» (PRD-D42): from the work log, «Робочий час», payments, adjustments, the roles and
/// «Параметри ціноутворення». While the month is open the work is paid by the item's current sewing rate, so fixing a
/// wrong rate fixes the pay; closing the month freezes everything.
/// </summary>
public sealed class PayrollSheets(IProductionDbContext db, IPayrollParameterReader parameters)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly PayrollPermissionsResponse NoPermissions = new(false, false, false, false);

    public static string Serialize(PayrollSnapshot snapshot) => JsonSerializer.Serialize(snapshot, Json);

    /// <summary>The month as it is now, or as it was closed.</summary>
    public async Task<PayrollSnapshot> ReadAsync(DateOnly month, CancellationToken cancellationToken)
    {
        var period = await db.PayrollMonths.AsNoTracking().SingleOrDefaultAsync(x => x.Month == month, cancellationToken);
        if (period is { IsClosed: true, Snapshot: { } json })
            return JsonSerializer.Deserialize<PayrollSnapshot>(json, Json)!;
        return await CalculateAsync(month, period, cancellationToken);
    }

    /// <summary>The month for this person: what they may do with it now.</summary>
    public static PayrollMonthResponse Present(PayrollMonthResponse month, ProductionActor actor, TimeProvider clock)
    {
        PayrollMonths.TryParse(month.Month, out var first);
        var started = first <= PayrollMonths.Current(clock);
        return month with
        {
            Permissions = new PayrollPermissionsResponse(
                actor.SeesPayroll && !month.IsClosed,
                actor.CanPay && !month.IsClosed && started,
                actor.CanPay && !month.IsClosed && started && !PayrollMessages.BlocksClosing(month.Warnings),
                actor.CanPay && month.IsClosed)
        };
    }

    private async Task<PayrollSnapshot> CalculateAsync(DateOnly month, PayrollMonth? period,
        CancellationToken cancellationToken)
    {
        var next = month.AddMonths(1);
        var employees = await db.Employees.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
        var work = await db.Work.AsNoTracking()
            .Where(x => x.WorkDate >= month && x.WorkDate < next && x.VoidedAt == null)
            .OrderBy(x => x.WorkDate).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        var orderIds = work.Select(x => x.OrderId).Distinct().ToList();
        var items = (await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => orderIds.Contains(x.Id))
                .ToListAsync(cancellationToken))
            .SelectMany(o => o.Items.Select(i => (OrderNumber: o.Number, Item: i)))
            .ToDictionary(x => x.Item.Id, x => (x.OrderNumber, x.Item.ModelName, x.Item.SewingRate));
        var minutes = (await db.TimeEntries.AsNoTracking()
                .Where(x => x.WorkDate >= month && x.WorkDate < next)
                .Select(x => new { x.EmployeeId, x.Minutes })
                .ToListAsync(cancellationToken))
            .GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.Sum(m => m.Minutes));
        var payments = await db.PayrollPayments.AsNoTracking().Where(x => x.Month == month)
            .OrderBy(x => x.PaidOn).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        var adjustments = await db.PayrollAdjustments.AsNoTracking().Where(x => x.Month == month)
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var issued = await db.PayrollIssued.AsNoTracking().Where(x => x.Month == month)
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Amount, cancellationToken);
        var rates = await ReadRatesAsync(cancellationToken);
        var roles = await db.PayrollRoles.AsNoTracking().ToDictionaryAsync(x => x.Role, x => x.EmployeeId, cancellationToken);

        var master = Person(roles.GetValueOrDefault(PayrollRoleKind.Master), employees, "майстер");
        var chief = Person(roles.GetValueOrDefault(PayrollRoleKind.Chief), employees, "начальник");
        var stitches = period?.Stitches ?? 0;
        var embroidery = period?.EmbroideryEmployeeId is { } embroideryId && employees.TryGetValue(embroideryId, out var e)
            ? new PayrollPersonResponse(e.Id, e.FullName, true)
            : stitches > 0 && chief is not null ? chief with { IsAssigned = false } : null;

        decimal RateOf(ProductionWork x) =>
            items.TryGetValue(x.ItemId, out var item) && item.SewingRate is > 0 ? item.SewingRate.Value : x.Rate;

        var result = PayrollCalculation.Calculate(new PayrollInputs(
            [.. employees.Values.Where(x => x.IsActive).Select(x => x.Id).Concat(minutes.Keys)],
            [.. work.Select(x => new PayrollWorkLine(x.EmployeeId, x.Kind, x.Quantity, RateOf(x)))],
            adjustments.Where(x => !x.IsVoided).GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.Sum(a => a.Amount)),
            payments.Where(x => !x.IsVoided).GroupBy(x => x.EmployeeId).ToDictionary(x => x.Key, x => x.Sum(p => p.Amount)),
            new PayrollPeople(master?.EmployeeId, chief?.EmployeeId, embroidery?.EmployeeId),
            stitches, rates));

        var lines = result.Lines.Select(x =>
            {
                var employee = employees.GetValueOrDefault(x.EmployeeId);
                return new PayrollLineResponse(x.EmployeeId, employee?.FullName ?? $"#{x.EmployeeId}",
                    employee?.Position ?? "", employee?.IsActive ?? false, minutes.GetValueOrDefault(x.EmployeeId),
                    x.Sewn, x.Sewing, x.Cut, x.Cutting, x.Bonus, x.Master, x.Chief, x.Embroidery, x.Other, x.Accrued,
                    x.Paid, x.Due, issued.TryGetValue(x.EmployeeId, out var amount) ? amount : null);
            })
            .OrderByDescending(x => x.IsActive).ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        var workRows = work.Select(x =>
        {
            var rate = RateOf(x);
            var amount = x.Kind == ProductionWorkKind.Sewing
                ? Money(x.Quantity * rate)
                : Money(x.Quantity * rate * (rates.Cutting ?? 0));
            var found = items.TryGetValue(x.ItemId, out var item);
            return new PayrollWorkRowResponse(x.Id, x.EmployeeId, x.WorkDate, x.OrderId, found ? item.OrderNumber : 0,
                found ? item.ModelName : "", x.Kind.ToString(), x.Quantity, rate, amount, x.OverPlan);
        }).ToList();

        var response = new PayrollMonthResponse(PayrollMonths.Format(month), false, null, null,
            new PayrollRatesResponse(rates.Bonus, rates.BonusThreshold, rates.Cutting, rates.Master, rates.Chief,
                rates.Embroidery),
            master, chief, stitches, embroidery, result.TotalSewing,
            [.. result.Warnings.Select(PayrollMessages.Warning)], lines, Totals(lines),
            [.. payments.Select(Map)], [.. adjustments.Select(Map)], NoPermissions);
        return new PayrollSnapshot(response, workRows);
    }

    public static PayrollPaymentResponse Map(PayrollPayment x) => new(x.Id, x.EmployeeId, x.EmployeeName, x.PaidOn,
        x.Kind.ToString(), x.Amount, x.Comment, x.CreatedAt, x.CreatedByName, x.VoidedAt, x.VoidedByName, x.VoidReason);

    public static PayrollAdjustmentResponse Map(PayrollAdjustment x) => new(x.Id, x.EmployeeId, x.EmployeeName,
        x.Amount, x.Reason, x.CreatedAt, x.CreatedByName, x.VoidedAt, x.VoidedByName, x.VoidReason);

    private static PayrollTotalsResponse Totals(IReadOnlyList<PayrollLineResponse> lines) => new(
        lines.Sum(x => x.Minutes), lines.Sum(x => x.Sewing), lines.Sum(x => x.Cutting), lines.Sum(x => x.Bonus),
        lines.Sum(x => x.Master), lines.Sum(x => x.Chief), lines.Sum(x => x.Embroidery), lines.Sum(x => x.Other),
        lines.Sum(x => x.Accrued), lines.Sum(x => x.Paid), lines.Sum(x => x.Due), lines.Sum(x => x.Issued ?? 0));

    /// <summary>The chosen person, or the only active worker whose position names the role.</summary>
    private static PayrollPersonResponse? Person(int? chosen, IReadOnlyDictionary<int, WorkshopEmployee> employees,
        string position)
    {
        if (chosen is { } id && employees.TryGetValue(id, out var employee))
            return new PayrollPersonResponse(employee.Id, employee.FullName, true);
        var found = employees.Values
            .Where(x => x.IsActive && x.Position.Contains(position, StringComparison.CurrentCultureIgnoreCase))
            .Take(2).ToList();
        return found.Count == 1 ? new PayrollPersonResponse(found[0].Id, found[0].FullName, false) : null;
    }

    private async Task<PayrollRates> ReadRatesAsync(CancellationToken cancellationToken)
    {
        var values = (await parameters.ReadAsync(
            [
                PayrollParameterCodes.SeamstressBonus, PayrollParameterCodes.SeamstressBonusThreshold,
                PayrollParameterCodes.CuttingRate, PayrollParameterCodes.MasterRate,
                PayrollParameterCodes.ShopManagerRate, PayrollParameterCodes.EmbroideryRate
            ], cancellationToken)).ToDictionary(x => x.Code, x => x.Value);
        decimal? Value(string code) => values.GetValueOrDefault(code);
        return new PayrollRates(Value(PayrollParameterCodes.SeamstressBonus),
            Value(PayrollParameterCodes.SeamstressBonusThreshold), Value(PayrollParameterCodes.CuttingRate),
            Value(PayrollParameterCodes.MasterRate), Value(PayrollParameterCodes.ShopManagerRate),
            Value(PayrollParameterCodes.EmbroideryRate));
    }

    private static decimal Money(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}

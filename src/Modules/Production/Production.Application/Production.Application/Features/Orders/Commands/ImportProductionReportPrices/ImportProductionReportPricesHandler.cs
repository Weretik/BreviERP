using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Orders.Commands.ImportProductionReportPrices.Rows;

namespace Production.Application.Features.Orders.Commands.ImportProductionReportPrices;

public sealed class ImportProductionReportPricesHandler(IProductionDbContext db)
    : ICommandHandler<ImportProductionReportPricesCommand, Result<ProductionReportPriceImportResult>>
{
    public async ValueTask<Result<ProductionReportPriceImportResult>> Handle(
        ImportProductionReportPricesCommand command, CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete)
            return Result<ProductionReportPriceImportResult>.Forbidden(OrderMessages.CannotImport);

        var legacyIds = command.Rows.Select(x => x.LegacyId.Trim()).ToArray();
        var orders = await db.Orders.Include(x => x.Items)
            .Where(x => x.LegacyId != null && legacyIds.Contains(x.LegacyId) && x.DeletedAt == null)
            .ToDictionaryAsync(x => x.LegacyId!, cancellationToken);

        var lines = new List<ProductionReportPriceImportLine>();
        var changes = new List<(ProductionItem Item, decimal Price)>();
        foreach (var row in command.Rows)
        {
            var legacyId = row.LegacyId.Trim();
            if (!orders.TryGetValue(legacyId, out var order))
            {
                lines.Add(new(null, legacyId, 0, "error", "Завдання не перенесено: спершу «Імпорт» завдань."));
                continue;
            }

            var byPosition = order.Items.ToDictionary(x => x.Position);
            var missing = row.Items.Select(x => x.Position).FirstOrDefault(x => !byPosition.ContainsKey(x));
            if (missing > 0)
            {
                lines.Add(new(order.Number, legacyId, 0, "error", $"У завданні немає позиції {missing}."));
                continue;
            }

            var fills = row.Items.Where(x => byPosition[x.Position].ReportUnitPrice is null)
                .Select(x => (byPosition[x.Position], x.UnitPrice)).ToList();
            changes.AddRange(fills);
            lines.Add(new(order.Number, legacyId, fills.Count, fills.Count > 0 ? "updated" : "unchanged", null));
        }

        var errors = lines.Count(x => x.Action == "error");
        if (!command.DryRun && errors == 0)
        {
            // Nothing is changed until every row is known to be fine, so an error leaves the orders as they were.
            foreach (var (item, price) in changes) item.FillReportPrice(price);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new ProductionReportPriceImportResult(command.DryRun || errors > 0,
            lines.Count(x => x.Action == "updated"), lines.Count(x => x.Action == "unchanged"), errors, lines));
    }
}

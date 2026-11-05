using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;

namespace Production.Application.Orders.ImportReportPrices;

/// <summary>
/// «Ціни КП» for orders moved by «Імпорт»: the old «Цех» kept no КП price, so «Звіти цеху» could not value their
/// output. Only an item without a price takes one; <paramref name="DryRun"/> only checks, and a repeat changes nothing.
/// </summary>
public sealed record ImportProductionReportPricesCommand(
    ProductionActor Actor, IReadOnlyList<ProductionReportPriceImportRow> Rows, bool DryRun)
    : ICommand<Result<ProductionReportPriceImportResult>>;

public sealed class ImportProductionReportPricesCommandValidator
    : AbstractValidator<ImportProductionReportPricesCommand>
{
    public ImportProductionReportPricesCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= 1000)
            .WithMessage("У файлі має бути від 1 до 1000 завдань.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
            row.RuleFor(x => x.Items).NotNull();
            row.RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.Position).GreaterThan(0);
                item.RuleFor(x => x.UnitPrice).GreaterThan(0).LessThan(10_000_000m)
                    .WithMessage("Ціна має бути більшою за нуль.");
            }).When(x => x.Items is not null);
        }).When(x => x.Rows is not null);
    }
}

public sealed class ImportProductionReportPricesCommandHandler(IProductionDbContext db)
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

using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Domain.Orders.Enums;
using Production.Domain.Orders.Rules;
using Production.Application.Features.Orders.Commands.ImportProductionProgress.Rows;

namespace Production.Application.Features.Orders.Commands.ImportProductionProgress;

public sealed class ImportProductionProgressHandler(IProductionDbContext db, TimeProvider clock)
    : ICommandHandler<ImportProductionProgressCommand, Result<ProductionProgressImportResult>>
{
    private const string Transfer = "Перенесення зі старого «Цеху»";

    public async ValueTask<Result<ProductionProgressImportResult>> Handle(ImportProductionProgressCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete)
            return Result<ProductionProgressImportResult>.Forbidden(OrderMessages.CannotImport);

        var legacyIds = command.Rows.Select(x => x.LegacyId.Trim()).ToArray();
        var orders = await db.Orders.Include(x => x.Items).Include(x => x.Shipments)
            .Where(x => x.LegacyId != null && legacyIds.Contains(x.LegacyId) && x.DeletedAt == null)
            .ToDictionaryAsync(x => x.LegacyId!, cancellationToken);
        var orderIds = orders.Values.Select(x => x.Id).ToArray();
        var fileUrls = (await db.OrderFiles.AsNoTracking()
                .Where(x => orderIds.Contains(x.OrderId) && x.DeletedAt == null)
                .Select(x => new { x.OrderId, x.Url }).ToListAsync(cancellationToken))
            .Select(x => (x.OrderId, x.Url)).ToHashSet();
        var deliveries = await db.TelegramDeliveries.Where(x => orderIds.Contains(x.OrderId))
            .ToDictionaryAsync(x => x.OrderId, cancellationToken);

        var now = clock.GetUtcNow();
        var lines = new List<ProductionProgressImportLine>();
        var changes = new List<Action>();
        foreach (var row in command.Rows)
        {
            var legacyId = row.LegacyId.Trim();
            if (!orders.TryGetValue(legacyId, out var order))
            {
                lines.Add(new ProductionProgressImportLine(null, legacyId, 0, 0, "error",
                    "Завдання не перенесено: спершу «Імпорт» завдань."));
                continue;
            }

            var byPosition = order.Items.ToDictionary(x => x.Position);
            var missing = row.Items.Select(x => x.Position).Concat(row.Files.Select(x => x.Position).OfType<int>())
                .FirstOrDefault(x => !byPosition.ContainsKey(x));
            if (missing > 0)
            {
                lines.Add(new ProductionProgressImportLine(order.Number, legacyId, 0, 0, "error",
                    $"У завданні немає позиції {missing}."));
                continue;
            }

            var stageChanges = row.Items
                .Select(x => (Item: byPosition[x.Position], Stages: ProductionStages.Normalize(
                    x.Stages.Select(stage => Enum.Parse<ProductionStage>(stage, ignoreCase: true)))))
                .Where(x => !x.Stages.SequenceEqual(x.Item.Stages)).ToList();
            var newFiles = row.Files.Where(x => !fileUrls.Contains((order.Id, x.Url.Trim()))).ToList();
            deliveries.TryGetValue(order.Id, out var delivery);
            var marksTelegram = row.TelegramSent && delivery is not { Status: TelegramDeliveryStatus.Sent };
            var changed = stageChanges.Count > 0 || newFiles.Count > 0 || marksTelegram;
            lines.Add(new ProductionProgressImportLine(order.Number, legacyId, stageChanges.Count, newFiles.Count,
                changed ? "updated" : "unchanged", null));
            if (!changed) continue;
            changes.Add(() =>
            {
                // The items come from the order itself, so a stage change cannot miss its item.
                foreach (var (item, stages) in stageChanges) _ = order.SetItemStages(item.Id, stages, Transfer, now).Value;
                foreach (var file in newFiles)
                {
                    var itemId = file.Position is { } position ? byPosition[position].Id : (int?)null;
                    db.OrderFiles.Add(new ProductionOrderFile(order.Id, itemId,
                        Enum.Parse<ProductionFileKind>(file.Kind, ignoreCase: true), file.Name.Trim(),
                        file.ContentType.Trim(), file.Bytes, file.Url.Trim(), null,
                        string.IsNullOrWhiteSpace(file.CreatedByName) ? Transfer : file.CreatedByName, file.CreatedAt));
                    fileUrls.Add((order.Id, file.Url.Trim()));
                }

                if (marksTelegram)
                {
                    if (delivery is null)
                    {
                        delivery = new ProductionTelegramDelivery(order.Id);
                        db.TelegramDeliveries.Add(delivery);
                        deliveries[order.Id] = delivery;
                    }

                    delivery.MarkSentByOldApp(row.TelegramSentAt);
                }

                if (newFiles.Count > 0)
                    order.Record("Перенесено", $"Файлів зі старого «Цеху»: {newFiles.Count}", Transfer, now);
            });
        }

        var errors = lines.Count(x => x.Action == "error");
        if (!command.DryRun && errors == 0)
        {
            // Nothing is changed until every row is known to be fine, so an error leaves the orders as they were.
            foreach (var change in changes) change();
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new ProductionProgressImportResult(command.DryRun || errors > 0,
            lines.Count(x => x.Action == "updated"), lines.Count(x => x.Action == "unchanged"), errors, lines));
    }
}

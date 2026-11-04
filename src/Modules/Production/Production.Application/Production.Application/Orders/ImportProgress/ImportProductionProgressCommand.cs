using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Orders;

namespace Production.Application.Orders.ImportProgress;

/// <summary>
/// Moves the етапи, file links and the Telegram mark of the old «Цех» onto orders already moved by «Імпорт».
/// <paramref name="DryRun"/> only checks. A repeat changes nothing: stages are set, not added, and a file link
/// already on the order is skipped.
/// </summary>
public sealed record ImportProductionProgressCommand(
    ProductionActor Actor, IReadOnlyList<ProductionProgressImportRow> Rows, bool DryRun)
    : ICommand<Result<ProductionProgressImportResult>>;

public sealed class ImportProductionProgressCommandValidator : AbstractValidator<ImportProductionProgressCommand>
{
    public ImportProductionProgressCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= 1000)
            .WithMessage("У файлі має бути від 1 до 1000 завдань.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
            row.RuleFor(x => x.Items).NotNull();
            row.RuleFor(x => x.Files).NotNull();
            row.RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(x => x.Position).GreaterThan(0);
                item.RuleFor(x => x.Stages).NotNull();
                item.RuleForEach(x => x.Stages)
                    .Must(x => Enum.TryParse<ProductionStage>(x, true, out var stage) && Enum.IsDefined(stage))
                    .WithMessage("Невідомий етап.");
            }).When(x => x.Items is not null);
            row.RuleForEach(x => x.Files).ChildRules(file =>
            {
                file.RuleFor(x => x.Position).GreaterThan(0).When(x => x.Position is not null);
                file.RuleFor(x => x.Kind)
                    .Must(x => Enum.TryParse<ProductionFileKind>(x, true, out var kind) && Enum.IsDefined(kind))
                    .WithMessage("Невідомий вид файлу.");
                file.RuleFor(x => x.Name).NotEmpty().MaximumLength(ProductionOrderFile.NameMaxLength);
                file.RuleFor(x => x.ContentType).NotEmpty().MaximumLength(ProductionOrderFile.ContentTypeMaxLength);
                file.RuleFor(x => x.Bytes).GreaterThanOrEqualTo(0);
                file.RuleFor(x => x.Url).NotEmpty().MaximumLength(ProductionOrderFile.UrlMaxLength)
                    .Must(x => Uri.TryCreate(x, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
                    .WithMessage("Посилання на файл має бути https.");
                file.RuleFor(x => x.CreatedByName).MaximumLength(ProductionOrder.PersonMaxLength);
            }).When(x => x.Files is not null);
        }).When(x => x.Rows is not null);
    }
}

public sealed class ImportProductionProgressCommandHandler(IProductionDbContext db, TimeProvider clock)
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
                foreach (var (item, stages) in stageChanges) order.SetItemStages(item.Id, stages, Transfer, now);
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

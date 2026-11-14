using Production.Application.Abstractions;
using Production.Domain;
using Production.Domain.Orders;
using Production.Domain.Work;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Work.Commands.ImportProductionWork;

public sealed class ImportProductionWorkHandler(IProductionDbContext db)
    : ICommandHandler<ImportProductionWorkCommand, Result<ProductionWorkImportResult>>
{
    private const string Transfer = "Перенесення зі старого «Цеху»";

    public async ValueTask<Result<ProductionWorkImportResult>> Handle(ImportProductionWorkCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete)
            return Result<ProductionWorkImportResult>.Forbidden(WorkMessages.CannotImport);

        var orderIds = command.Rows.Select(x => x.OrderLegacyId.Trim()).Distinct().ToArray();
        var orders = await db.Orders.AsNoTracking().Include(x => x.Items)
            .Where(x => x.LegacyId != null && orderIds.Contains(x.LegacyId) && x.DeletedAt == null)
            .ToDictionaryAsync(x => x.LegacyId!, cancellationToken);
        var employees = (await db.Employees.AsNoTracking().ToListAsync(cancellationToken))
            .GroupBy(x => x.NameKey).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.IsActive).First());
        var workIds = command.Rows.Select(x => x.LegacyId.Trim()).ToArray();
        var moved = (await db.Work.AsNoTracking().Where(x => x.LegacyId != null && workIds.Contains(x.LegacyId))
            .Select(x => x.LegacyId!).ToListAsync(cancellationToken)).ToHashSet();

        var lines = new List<ProductionWorkImportLine>();
        var records = new List<ProductionWork>();
        foreach (var row in command.Rows)
        {
            var legacyId = row.LegacyId.Trim();
            orders.TryGetValue(row.OrderLegacyId.Trim(), out var order);
            ProductionWorkImportLine Line(string action, string? error) => new(legacyId, order?.Number, row.Position,
                row.EmployeeName.Trim(), row.Kind, row.Quantity, action, error);

            if (moved.Contains(legacyId) || !records.TrueForAll(x => x.LegacyId != legacyId))
            {
                lines.Add(Line("unchanged", null));
                continue;
            }

            var item = order?.Items.FirstOrDefault(x => x.Position == row.Position);
            var error = order is null ? "Завдання не перенесено: спершу «Імпорт» завдань."
                : item is null ? $"У завданні немає позиції {row.Position}."
                : !employees.TryGetValue(ProductionText.Key(row.EmployeeName), out _)
                    ? "Працівника немає в «Працівники цеху»: спершу перенесіть працівників."
                    : row.VoidedAt is not null && string.IsNullOrWhiteSpace(row.VoidReason)
                        ? "Сторно без причини."
                        : null;
            if (error is not null)
            {
                lines.Add(Line("error", error));
                continue;
            }

            var employee = employees[ProductionText.Key(row.EmployeeName)];
            var work = ProductionWork.Create(new WorkTarget(order!.Id, item!.Id, employee.Id, employee.FullName),
                new WorkFacts(Enum.Parse<ProductionWorkKind>(row.Kind), row.Quantity, row.WorkDate, row.OverPlan,
                    row.Reason, row.Comment, row.Rate), Transfer, row.At, legacyId);
            var voided = work.IsSuccess && row.VoidedAt is { } voidedAt
                ? work.Value.Void(row.VoidReason!, Transfer, voidedAt)
                : DomainResult.Success();
            if ((work.IsFailure ? work.Error : voided.Error) is { } invalid)
            {
                lines.Add(Line("error", invalid.Message));
                continue;
            }

            records.Add(work.Value);
            lines.Add(Line("created", null));
        }

        var errors = lines.Count(x => x.Action == "error");
        if (!command.DryRun && errors == 0 && records.Count > 0)
        {
            db.Work.AddRange(records);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new ProductionWorkImportResult(command.DryRun || errors > 0,
            lines.Count(x => x.Action == "created"), lines.Count(x => x.Action == "unchanged"), errors, lines));
    }
}

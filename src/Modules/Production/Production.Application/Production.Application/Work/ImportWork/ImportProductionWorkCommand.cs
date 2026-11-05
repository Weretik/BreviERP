using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain;
using Production.Domain.Orders;
using Production.Domain.Work;

namespace Production.Application.Work.ImportWork;

/// <summary>
/// Moves «Журнал робіт» of the old «Цех» onto orders already moved by «Імпорт» (PRD-D40). The item is found by the
/// order's legacy id and the position, the worker by full name. All or nothing; a repeat skips records already moved.
/// </summary>
public sealed record ImportProductionWorkCommand(ProductionActor Actor, IReadOnlyList<ProductionWorkImportRow> Rows,
    bool DryRun) : ICommand<Result<ProductionWorkImportResult>>;

public sealed class ImportProductionWorkCommandValidator : AbstractValidator<ImportProductionWorkCommand>
{
    public ImportProductionWorkCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= 5000)
            .WithMessage("У файлі має бути від 1 до 5000 записів.");
        RuleForEach(x => x.Rows).ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(ProductionWork.LegacyIdMaxLength);
            row.RuleFor(x => x.OrderLegacyId).NotEmpty().MaximumLength(ProductionOrder.LegacyIdMaxLength);
            row.RuleFor(x => x.Position).GreaterThan(0);
            row.RuleFor(x => x.EmployeeName).NotEmpty().MaximumLength(ProductionOrder.PersonMaxLength);
            row.RuleFor(x => x.Kind).Must(x => Enum.TryParse<ProductionWorkKind>(x, out var k) && Enum.IsDefined(k))
                .WithMessage("Невідомий вид роботи.");
            row.RuleFor(x => x.Quantity).InclusiveBetween(1, ProductionWork.MaxQuantity);
            row.RuleFor(x => x.Rate).GreaterThan(0);
            row.RuleFor(x => x.Reason).Must(OverPlanReasons.IsKnown).WithMessage(WorkMessages.Reason)
                .When(x => x.OverPlan);
            row.RuleFor(x => x.Comment).MaximumLength(ProductionWork.CommentMaxLength);
            row.RuleFor(x => x.VoidReason).MaximumLength(ProductionWork.ReasonMaxLength);
        }).When(x => x.Rows is not null);
    }
}

public sealed class ImportProductionWorkCommandHandler(IProductionDbContext db)
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
            var work = new ProductionWork(new WorkTarget(order!.Id, item!.Id, employee.Id, employee.FullName),
                new WorkFacts(Enum.Parse<ProductionWorkKind>(row.Kind), row.Quantity, row.WorkDate, row.OverPlan,
                    row.Reason, row.Comment, row.Rate), Transfer, row.At, legacyId);
            if (row.VoidedAt is { } voidedAt) work.Void(row.VoidReason!, Transfer, voidedAt);
            records.Add(work);
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

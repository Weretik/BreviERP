using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Domain.Archive;

namespace Production.Application.Archive.ImportArchive;

/// <summary>
/// Moves the old «ГРАФИК работы» and the old requests into «Архів цеху» (Admin). <paramref name="DryRun"/> only
/// checks; a line already moved is skipped, so a repeat or a fresh export adds only what is new.
/// </summary>
public sealed record ImportWorkshopArchiveCommand(
    ProductionActor Actor, IReadOnlyList<WorkshopArchiveImportRow> Rows, bool DryRun)
    : ICommand<Result<WorkshopArchiveImportResult>>;

public sealed class ImportWorkshopArchiveCommandValidator : AbstractValidator<ImportWorkshopArchiveCommand>
{
    public ImportWorkshopArchiveCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Rows).NotNull().Must(x => x.Count is > 0 and <= ArchiveMessages.MaxImportRows)
            .WithMessage($"У файлі має бути від 1 до {ArchiveMessages.MaxImportRows} рядків.");
    }
}

public sealed class ImportWorkshopArchiveCommandHandler(IProductionDbContext db)
    : ICommandHandler<ImportWorkshopArchiveCommand, Result<WorkshopArchiveImportResult>>
{
    public async ValueTask<Result<WorkshopArchiveImportResult>> Handle(ImportWorkshopArchiveCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanDelete)
            return Result<WorkshopArchiveImportResult>.Forbidden(ArchiveMessages.CannotImport);

        var known = (await db.Archive.AsNoTracking().Select(x => x.SourceKey).ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.Ordinal);
        var errors = new List<WorkshopArchiveImportError>();
        var created = new List<WorkshopArchiveRecord>();
        var unchanged = 0;
        foreach (var row in command.Rows)
        {
            var key = row.SourceKey?.Trim() ?? string.Empty;
            var error = Problem(row, key);
            if (error is not null)
            {
                errors.Add(new(key, error));
                continue;
            }

            if (!known.Add(key))
            {
                unchanged++;
                continue;
            }

            created.Add(new WorkshopArchiveRecord(Enum.Parse<WorkshopArchiveKind>(row.Kind, ignoreCase: true), key,
                row.SourceRow, new WorkshopArchiveEntry(row.Date, row.DateText, row.Number, row.Client, row.Product,
                    row.Quantity, row.Fabric, row.Status, row.Notes,
                    [.. (row.Fields ?? []).Select(x => new WorkshopArchiveField(x.Label ?? "", x.Value ?? ""))],
                    [.. (row.Links ?? []).Select(x => new WorkshopArchiveLink(x.Label ?? "", x.Url ?? ""))])));
        }

        var dryRun = command.DryRun || errors.Count > 0;
        if (!dryRun && created.Count > 0)
        {
            db.Archive.AddRange(created);
            await db.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new WorkshopArchiveImportResult(dryRun, created.Count, unchanged, errors.Count,
            errors.Take(50).ToList()));
    }

    private static string? Problem(WorkshopArchiveImportRow row, string key)
    {
        if (key.Length is 0 or > WorkshopArchiveRecord.SourceKeyMaxLength) return "Немає ключа рядка.";
        if (!Enum.TryParse<WorkshopArchiveKind>(row.Kind, true, out var kind) || !Enum.IsDefined(kind))
            return ArchiveMessages.Kind;
        if (row.SourceRow <= 0) return "Немає номера рядка.";
        if ((row.Links ?? []).Any(x => !Uri.TryCreate(x.Url, UriKind.Absolute, out var uri) ||
                                       uri.Scheme != Uri.UriSchemeHttps))
            return "Посилання має бути https.";
        return null;
    }
}

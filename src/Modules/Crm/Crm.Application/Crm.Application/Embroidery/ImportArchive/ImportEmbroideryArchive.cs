using Crm.Application.Embroidery.Access;
using Crm.Application.Embroidery.Calculate;
using Crm.Application.Embroidery.Contracts;
using Crm.Application.Embroidery.Mapping;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Embroidery.SaveCalculation;
using Crm.Application.Embroidery.SaveSettings;
using Crm.Application.Quotes.Staff;
using Crm.Domain.Embroidery.Calculation;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;

namespace Crm.Application.Embroidery.ImportArchive;

/// <summary>
/// «Імпорт архіву» (Director and Admin): moves the old calculator's saved calculations into «Збережені» with their
/// own dates, authors and frozen rates. Each record is priced with its old rates and must give the old saved prices to
/// the kopeck; nothing is priced with today's rates. A record already moved stays as it is; a list with any error
/// saves nothing. With <c>DryRun</c> only shows what would happen.
/// </summary>
public sealed record ImportEmbroideryArchiveCommand(EmbroideryActor Actor, IReadOnlyList<EmbroideryArchiveRow> Rows,
    bool DryRun) : ICommand<Result<EmbroideryArchiveImportResult>>;

public sealed class ImportEmbroideryArchiveCommandHandler(IEmbroideryStore store, IQuoteStaffReader staff)
    : ICommandHandler<ImportEmbroideryArchiveCommand, Result<EmbroideryArchiveImportResult>>
{
    public async ValueTask<Result<EmbroideryArchiveImportResult>> Handle(ImportEmbroideryArchiveCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.SeesCost) return Result.Forbidden(EmbroideryMessages.ImportForbidden);
        var actor = await staff.FindAsync(command.Actor.UserId, cancellationToken);
        if (actor is null) return Result.Forbidden(EmbroideryMessages.ActorUnknown);

        var moved = (await store.EmbroideryCalculations.AsNoTracking().Where(x => x.LegacyId != null)
            .Select(x => x.LegacyId!).ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);
        var people = (await staff.ListAsync(cancellationToken)).Where(x => !string.IsNullOrWhiteSpace(x.Email))
            .GroupBy(x => x.Email!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var lines = new List<EmbroideryArchiveImportLine>(command.Rows.Count);
        var created = new List<(int Line, EmbroiderySavedCalculation Saved)>();

        foreach (var row in command.Rows)
        {
            var legacyId = row.LegacyId.Trim();
            var name = EmbroiderySavedCalculation.NormalizeName(row.Name);
            var email = row.CreatedByEmail?.Trim();
            var author = email is { Length: > 0 } ? people.GetValueOrDefault(email) : null;
            var authorName = author?.Name ?? (email is { Length: > 0 } ? email : actor.Name);
            EmbroideryArchiveImportLine Line(string action, string? message) =>
                new(legacyId, name, row.CreatedAt, authorName, action, message, null);

            if (!seen.Add(legacyId))
            {
                lines.Add(Line("error", EmbroideryMessages.ImportDuplicate));
                continue;
            }

            if (moved.Contains(legacyId))
            {
                lines.Add(Line("unchanged", null));
                continue;
            }

            var input = row.Calculation.ToDomain();
            var snapshot = new EmbroideryRateSnapshot(row.StitchRate, row.Settings.ToContent());
            var calculation = EmbroideryCalculator.Calculate(input, snapshot.Settings, snapshot.StitchRate);
            if (calculation.Result is not { } result)
            {
                lines.Add(Line("error", calculation.Error));
                continue;
            }

            if (!SamePrices(result.Prices, row.Prices))
            {
                lines.Add(Line("error", EmbroideryMessages.ImportPricesDiffer));
                continue;
            }

            created.Add((lines.Count, EmbroiderySavedCalculation.Transferred(legacyId, row.Name, input, snapshot,
                result, author?.UserId ?? actor.UserId, authorName, row.CreatedAt)));
            lines.Add(Line("created", null));
        }

        var errors = lines.Count(x => x.Action == "error");
        var apply = !command.DryRun && errors == 0;
        if (apply && created.Count > 0)
        {
            store.EmbroideryCalculations.AddRange(created.OrderBy(x => x.Saved.CreatedAt).Select(x => x.Saved));
            await store.SaveChangesAsync(cancellationToken);
            foreach (var (line, saved) in created)
                lines[line] = lines[line] with { Number = EmbroiderySavedCalculation.NumberOf(saved.Id) };
        }

        return Result.Success(new EmbroideryArchiveImportResult(!apply, created.Count,
            lines.Count(x => x.Action == "unchanged"), errors, lines));
    }

    private static bool SamePrices(IReadOnlyList<EmbroideryPrice> computed, IReadOnlyList<EmbroideryArchivePrice> old) =>
        computed.Count == old.Count && computed.Zip(old).All(x =>
            x.First.Unit == x.Second.Unit && x.First.Total == x.Second.Total && x.First.RepeatUnit == x.Second.RepeatUnit);
}

public sealed class ImportEmbroideryArchiveCommandValidator : AbstractValidator<ImportEmbroideryArchiveCommand>
{
    public const int MaxRows = 2000;

    public ImportEmbroideryArchiveCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Rows).NotNull().Must(x => x is { Count: > 0 and <= MaxRows })
            .WithMessage($"У файлі має бути від 1 до {MaxRows} розрахунків.");
        RuleForEach(x => x.Rows).NotNull().ChildRules(row =>
        {
            row.RuleFor(x => x.LegacyId).NotEmpty().MaximumLength(EmbroiderySavedCalculation.LegacyIdMaxLength);
            row.RuleFor(x => x.Name).MaximumLength(SaveEmbroideryCalculationCommandValidator.RawNameMaxLength);
            row.RuleFor(x => x.CreatedByEmail).MaximumLength(EmbroiderySavedCalculation.PersonNameMaxLength);
            row.RuleFor(x => x.CreatedAt).NotEqual(default(DateTimeOffset));
            row.RuleFor(x => x.StitchRate).GreaterThan(0);
            row.RuleFor(x => x.Calculation).NotNull().SetValidator(new CalculateEmbroideryInputValidator());
            row.RuleFor(x => x.Settings).NotNull().SetValidator(new SaveEmbroiderySettingsInputValidator(EmbroiderySettingsRules.LegacyMarkupCount));
            row.RuleFor(x => x.Prices).NotNull().Must(x => x is { Count: > 0 })
                .WithMessage("Немає збережених цін старого розрахунку.");
        });
    }
}

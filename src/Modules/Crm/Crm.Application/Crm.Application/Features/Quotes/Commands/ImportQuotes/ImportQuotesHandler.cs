using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Domain.Quotes.ValueObjects;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Commands.ImportQuotes;

public sealed class ImportQuotesHandler(
    IQuoteStore store, IQuoteStaffReader staff, IQuoteImportLookup lookup, TimeProvider clock)
    : ICommandHandler<ImportQuotesCommand, Result<QuoteImportReport>>
{
    public const string Note = "Перенесено з калькулятора";
    private const string OldNumberNote = Note + ", старий номер ";

    public async ValueTask<Result<QuoteImportReport>> Handle(ImportQuotesCommand command,
        CancellationToken cancellationToken)
    {
        var importer = await staff.PersonAsync(command.Actor, cancellationToken);
        if (importer is null) return Result.Forbidden(QuoteMessages.ActorUnknown);

        var numbers = command.Quotes.Select(x => QuoteImportNumber.Parse(x.Number)).ToArray();
        var imported = await ImportedNumbersAsync(cancellationToken);
        var months = numbers.Select(x => (x.Year, x.Month)).Distinct().ToArray();
        var taken = await TakenAsync(months, cancellationToken);
        var next = months.ToDictionary(x => x, x => Math.Max(
            taken.Where(n => n.Year == x.Year && n.Month == x.Month).Select(n => n.Sequence).DefaultIfEmpty().Max(),
            numbers.Where(n => n.Year == x.Year && n.Month == x.Month).Max(n => n.Sequence)));

        var keepinIds = command.Quotes.Select(x => x.KeepinClientId?.Trim()).OfType<string>().Where(x => x.Length > 0)
            .Distinct().ToArray();
        var mapper = new QuoteImportMapper(await lookup.LoadAsync(cancellationToken),
            await staff.ListAsync(cancellationToken), await store.FindKeepinCustomersAsync(keepinIds, cancellationToken),
            importer);

        var now = clock.GetUtcNow();
        var takenNumbers = taken.Select(x => x.ToString()).ToHashSet();
        var quotes = new List<Quote>();
        var skipped = new List<string>();
        var renumbered = new List<QuoteImportRenumbered>();
        var warnings = new List<QuoteImportWarning>();
        for (var index = 0; index < command.Quotes.Count; index++)
        {
            var input = command.Quotes[index];
            var number = numbers[index];
            if (imported.Contains(number.ToString()))
            {
                skipped.Add(number.ToString());
                continue;
            }

            var note = Note;
            if (takenNumbers.Contains(number.ToString()))
            {
                var month = (number.Year, number.Month);
                var free = new QuoteNumber(number.Year, number.Month, next[month] = next[month] + 1);
                renumbered.Add(new QuoteImportRenumbered(number.ToString(), free.ToString()));
                note = OldNumberNote + number;
                number = free;
            }

            var mapped = mapper.Map(input, number, note, now, warnings);
            if (mapped.IsSuccess) quotes.Add(mapped.Value);
            else warnings.Add(new(number.ToString(), null, $"КП не перенесено: {mapped.Error!.Message}"));
        }

        var applied = false;
        if (!command.DryRun && quotes.Count > 0)
        {
            store.Quotes.AddRange(quotes);
            try
            {
                await store.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (store.IsDuplicateNumber(exception))
            {
                return Result.Conflict("Поки йшов перенос, хтось створив КП з тим самим номером. Повторіть пробний прогон.");
            }

            applied = true;
        }

        return Result.Success(new QuoteImportReport(command.DryRun, applied, command.Quotes.Count, quotes.Count,
            quotes.Count(x => x.Kind == QuoteKind.Request), quotes.Count(x => x.Kind == QuoteKind.Introduction),
            quotes.Sum(x => x.Items.Count), skipped, renumbered, mapper.UnknownPeople, warnings));
    }

    /// <summary>Calculator numbers already transferred: the number of an imported КП, or its old number when it changed.</summary>
    private async Task<HashSet<string>> ImportedNumbersAsync(CancellationToken cancellationToken)
    {
        var rows = await store.QuoteEvents.AsNoTracking()
            .Where(x => x.Kind == QuoteEventKind.Imported)
            .Join(store.Quotes, x => x.QuoteId, x => x.Id, (e, q) => new { q.Number, e.Details })
            .ToListAsync(cancellationToken);
        return rows.Select(x => x.Details is { } details && details.StartsWith(OldNumberNote, StringComparison.Ordinal)
                ? details[OldNumberNote.Length..]
                : x.Number)
            .ToHashSet();
    }

    private async Task<IReadOnlyList<QuoteNumber>> TakenAsync(IReadOnlyList<(int Year, int Month)> months,
        CancellationToken cancellationToken)
    {
        var years = months.Select(x => x.Year).Distinct().ToArray();
        var rows = await store.Quotes.AsNoTracking()
            .Where(x => years.Contains(x.NumberYear))
            .Select(x => new { x.NumberYear, x.NumberMonth, x.NumberSequence })
            .ToListAsync(cancellationToken);
        return rows.Where(x => months.Contains((x.NumberYear, x.NumberMonth)))
            .Select(x => new QuoteNumber(x.NumberYear, x.NumberMonth, x.NumberSequence)).ToArray();
    }
}

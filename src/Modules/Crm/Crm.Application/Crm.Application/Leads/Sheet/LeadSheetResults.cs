using Crm.Domain.Leads;

namespace Crm.Application.Leads.Sheet;

/// <summary>
/// Turns «Результат звонка» of the sheet into a call result of «Довідники» (owner, 2026-11-08). A result with the same
/// name is used as it is; the Russian values of the sheet are mapped to the default Ukrainian results, and when the
/// director renamed one, to the first active result with the same outcome. «Не звонили» is no call.
/// </summary>
public static class LeadSheetResults
{
    private static readonly Dictionary<string, (string Name, LeadOutcome Outcome)?> Aliases =
        new(StringComparer.Ordinal)
        {
            ["не звонили"] = null,
            ["не дозвонились"] = ("Не дозвонились", LeadOutcome.Retry),
            ["не отвечают"] = ("Не відповідають", LeadOutcome.Retry),
            ["узнать позже"] = ("Передзвонити", LeadOutcome.Callback),
            ["перезвонить"] = ("Передзвонити", LeadOutcome.Callback),
            ["вышли на ответственного"] = ("Вийшли на відповідального", LeadOutcome.Talking),
            ["отправить предложение"] = ("Надіслали пропозицію", LeadOutcome.Offer),
            ["отправили предложение"] = ("Надіслали пропозицію", LeadOutcome.Offer),
            ["отправили на почту"] = ("Надіслали пропозицію", LeadOutcome.Offer),
            ["нужен расчёт"] = ("Надіслали пропозицію", LeadOutcome.Offer),
            ["нужен расчет"] = ("Надіслали пропозицію", LeadOutcome.Offer),
            ["потенциальный заказ"] = ("Надіслали пропозицію", LeadOutcome.Offer),
            ["перенесен в crm"] = ("Перенесено в CRM", LeadOutcome.Client),
            ["перенесён в crm"] = ("Перенесено в CRM", LeadOutcome.Client),
            ["отказ"] = ("Відмова", LeadOutcome.Closed),
            ["потребности сейчас нет"] = ("Відмова", LeadOutcome.Closed),
            ["нецелевое предприятие"] = ("Нецільове підприємство", LeadOutcome.Closed),
        };

    /// <param name="Known">False when the text is neither a result nor a known sheet value.</param>
    /// <param name="Result">Null for a known value that is no call («Не звонили»).</param>
    /// <param name="Exact">False when the result name differs from the sheet text, which is then kept in the comment.</param>
    public sealed record Match(bool Known, LeadCallResult? Result, bool Exact);

    public static Match Find(string sheetResult, IReadOnlyList<LeadCallResult> active)
    {
        var key = Key(sheetResult);
        var same = active.FirstOrDefault(x => Key(x.Name) == key);
        if (same is not null) return new Match(true, same, true);
        if (!Aliases.TryGetValue(key, out var alias)) return new Match(false, null, false);
        if (alias is not { } target) return new Match(true, null, true);
        var result = active.FirstOrDefault(x => Key(x.Name) == Key(target.Name)) ??
                     active.Where(x => x.Outcome == target.Outcome).OrderBy(x => x.Position).FirstOrDefault();
        return new Match(result is not null, result, false);
    }

    /// <summary>The comment of an imported call: the sheet text when the result was mapped, then «Комментарий».</summary>
    public static string? Comment(string sheetResult, bool exact, string? comment) =>
        exact ? comment : string.Join(" ", new[] { $"У таблиці: «{sheetResult.Trim()}».", comment }.OfType<string>());

    private static string Key(string text) =>
        string.Join(' ', text.Trim().ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}

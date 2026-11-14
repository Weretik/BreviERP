using Crm.Domain.Leads;
using Crm.Application.Abstractions.Tasks;
using Crm.Domain.Leads.ValueObjects;

namespace Crm.Application.Features.Leads.Shared.Sheet;

/// <summary>
/// Finds the Brevi manager named in «Кто взял в работу» (owner, 2026-11-08). The sheet is in Russian and the accounts
/// in Ukrainian, so «Анастасия» finds «Анастасія Петренко»: the first word of the sheet name must equal a word of the
/// manager name, comparing letters the two languages spell differently as one. Two matching managers give none.
/// </summary>
public static class LeadSheetOwners
{
    public static LeadPerson? Find(string? sheetName, IReadOnlyList<TaskStaffMember> managers)
    {
        var first = Words(sheetName).FirstOrDefault();
        if (first is null) return null;
        var found = managers.Where(x => Words(x.Name).Contains(first)).Take(2).ToList();
        return found.Count == 1 ? new LeadPerson(found[0].UserId, found[0].Name) : null;
    }

    private static IEnumerable<string> Words(string? name) =>
        (name ?? "").Split([' ', ',', '.', '(', ')', '/', '-'], StringSplitOptions.RemoveEmptyEntries).Select(Fold);

    private static string Fold(string word) =>
        new(word.ToLowerInvariant().Select(c => c switch
        {
            'і' or 'ї' or 'ы' or 'й' => 'и',
            'є' or 'э' or 'ё' => 'е',
            'ґ' => 'г',
            _ => c,
        }).Where(c => c is not ('ъ' or 'ь' or '\'' or '’')).ToArray());
}

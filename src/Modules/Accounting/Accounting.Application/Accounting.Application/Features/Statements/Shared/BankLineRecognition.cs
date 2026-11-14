using System.Globalization;
using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Statements;
using Accounting.Domain.Statements.Enums;

namespace Accounting.Application.Features.Statements.Shared;

/// <summary>
/// Recognises the lines of a new statement: duplicates of records the books already have, transfers to other own
/// accounts, who each line is with and the category a confirmed counterparty puts on its lines.
/// </summary>
internal static class BankLineRecognition
{
    // Written out rather than taken from the uk-UA culture, which a container without ICU data does not have.
    private static readonly NumberFormatInfo Hryvnia = new() { NumberGroupSeparator = "\u00A0", NumberDecimalSeparator = "," };

    public static async Task RecogniseAsync(IAccountingDbContext db, IAccountingCustomerReader customers,
        IAccountingSupplierReader suppliers, FinanceAccount account, IReadOnlyList<BankStatementLine> lines,
        CancellationToken cancellationToken)
    {
        var readable = lines.Where(x => x.Error.Length == 0).ToList();
        if (readable.Count == 0) return;

        var duplicates = await BankDuplicates.LoadAsync(db, account, readable, cancellationToken);
        var accounts = await db.Accounts.AsNoTracking().ToListAsync(cancellationToken);
        var transferTo = accounts
            .Where(x => x.Id != account.Id && x.Active && x.Currency == account.Currency && x.Iban.Length > 0)
            .GroupBy(x => x.Iban).ToDictionary(x => x.Key, x => x.First());
        var parties = await BankParties.LoadAsync(db, customers, suppliers, readable, cancellationToken);

        var seen = new Dictionary<string, int>();
        foreach (var line in readable)
        {
            var (duplicate, note) = duplicates.Find(line);
            if (duplicate == BankDuplicate.None && line.Fingerprint is { } fingerprint)
            {
                if (seen.TryGetValue(fingerprint, out var first))
                    (duplicate, note) = (BankDuplicate.Exact, $"Повторює рядок {first} цієї виписки.");
                else seen[fingerprint] = line.RowNumber;
            }

            var transfer = transferTo.GetValueOrDefault(line.Iban);
            if (transfer is not null && duplicate == BankDuplicate.None)
                (duplicate, note) = duplicates.FindTransfer(line, transfer);

            var party = transfer is null ? parties.Find(line) : null;
            line.Recognise(duplicate, note, transfer?.Id, party?.State, party?.CategoryFor(line.Type));
        }
    }

    internal static string Money(long minor) => (minor / 100m).ToString("#,0.00", Hryvnia);
}

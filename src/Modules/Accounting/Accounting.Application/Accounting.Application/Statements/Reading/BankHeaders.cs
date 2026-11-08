using System.Text;

namespace Accounting.Application.Statements.Reading;

/// <summary>What a statement column holds.</summary>
internal enum BankColumn
{
    Date,
    Time,
    Amount,
    Debit,
    Credit,
    Currency,
    Counterparty,
    Edrpou,
    Iban,
    Purpose,
    Document,
    Reference,
    Direction,
}

/// <summary>
/// Header names a statement column may have — those of the old Calculator plus the PrivatBank card statement —
/// compared after <see cref="Normalize"/>.
/// </summary>
internal static class BankHeaders
{
    private static readonly Dictionary<string, BankColumn> Aliases = Build(new()
    {
        [BankColumn.Date] =
        [
            "дата", "дата операції", "дата операции", "дата транзакції", "дата проводки", "дата і час операції",
            "дата та час операції", "дата и время операции",
        ],
        [BankColumn.Time] = ["час", "время", "час операції", "время операции", "час проводки"],
        [BankColumn.Amount] =
        [
            "сума", "сумма", "сума операції", "сумма операции", "сума у валюті рахунку", "сума в валюті картки",
            "сума у валюті картки", "сума в валюті рахунку", "сумма в валюте карты",
        ],
        [BankColumn.Debit] = ["дебет", "списання", "списание", "витрата"],
        [BankColumn.Credit] = ["кредит", "зарахування", "зачисление", "дохід"],
        [BankColumn.Currency] =
            ["валюта", "валюта операції", "валюта счета", "валюта картки", "валюта рахунку", "валюта карты"],
        [BankColumn.Counterparty] =
            ["контрагент", "назва контрагента", "найменування контрагента", "наименование контрагента", "кореспондент"],
        [BankColumn.Edrpou] = ["єдрпоу", "єдрпоу рнокпп", "едрпоу", "іпн", "инн", "код контрагента"],
        [BankColumn.Iban] = ["iban", "iban контрагента", "рахунок контрагента", "счет контрагента"],
        [BankColumn.Purpose] =
        [
            "призначення платежу", "назначение платежа", "призначення", "назначение", "опис операції", "деталі платежу",
        ],
        [BankColumn.Document] =
            ["номер", "номер документа", "номер документу", "документ", "номер платіжного доручення"],
        [BankColumn.Reference] =
        [
            "reference", "референс", "id операції", "id операции", "ідентифікатор операції", "номер транзакції",
        ],
        [BankColumn.Direction] = ["тип операції", "напрямок", "направление"],
    });

    /// <summary>Columns of which a file may have only one; two of them make the file ambiguous.</summary>
    public static readonly BankColumn[] Single = [BankColumn.Amount, BankColumn.Debit, BankColumn.Credit, BankColumn.Currency];

    public static BankColumn? Of(string header) =>
        Aliases.TryGetValue(Normalize(header), out var column) ? column : null;

    /// <summary>
    /// Lower case, «№»/«#» read as «номер», Latin «i» as Ukrainian «і», every other sign a space, repeated spaces one.
    /// </summary>
    public static string Normalize(string? header)
    {
        if (string.IsNullOrWhiteSpace(header)) return string.Empty;
        var text = new StringBuilder(header.Length + 8);
        foreach (var c in header.ToLowerInvariant())
        {
            if (c is '№' or '#') text.Append(" номер ");
            else if (c == 'i') text.Append('і');
            else if (char.IsLetterOrDigit(c)) text.Append(c);
            else text.Append(' ');
        }

        return string.Join(' ', text.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static Dictionary<string, BankColumn> Build(Dictionary<BankColumn, string[]> map) =>
        map.SelectMany(x => x.Value.Select(name => (Name: Normalize(name), Column: x.Key)))
            .ToDictionary(x => x.Name, x => x.Column);
}

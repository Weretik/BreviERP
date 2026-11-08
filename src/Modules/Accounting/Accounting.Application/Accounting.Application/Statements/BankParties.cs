using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Statements;

namespace Accounting.Application.Statements;

/// <summary>A recognised counterparty and the category it puts on bank lines (only a confirmed one with auto-apply).</summary>
internal sealed record BankParty(CounterpartyState State, FinanceCategory? AutoCategory)
{
    public FinanceCategoryId? CategoryFor(CategoryDirection? type) =>
        AutoCategory is { Active: true, Scope: LedgerScope.Current } category && category.Direction == type
            ? category.Id
            : null;
}

/// <summary>Counterparties a line can be matched with: finance ones of this module, clients and suppliers.</summary>
internal sealed class BankParties
{
    private readonly Dictionary<string, BankParty> byCode = [];
    private readonly Dictionary<string, BankParty> byIban = [];
    private readonly Dictionary<string, BankParty> byName = [];

    public static async Task<BankParties> LoadAsync(IAccountingDbContext db, IAccountingCustomerReader customers,
        IAccountingSupplierReader suppliers, IReadOnlyList<BankStatementLine> lines, CancellationToken cancellationToken)
    {
        var result = new BankParties();
        var financial = await db.Counterparties.AsNoTracking().Where(x => x.Active && x.MergedIntoId == null)
            .OrderByDescending(x => x.Verified).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        var categoryIds = financial.Where(x => x.AutoApplyCategory && x.DefaultCategoryId != null)
            .Select(x => x.DefaultCategoryId!.Value).Distinct().ToList();
        var categories = await db.Categories.AsNoTracking().Where(x => categoryIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        BankParty Of(FinanceCounterparty x) => new(x.State,
            x is { AutoApplyCategory: true, Verified: true, DefaultCategoryId: { } id } ? categories.GetValueOrDefault(id) : null);

        var codes = lines.Select(x => x.Edrpou).Where(x => x.Length > 0).Distinct().ToList();
        foreach (var x in financial.Where(x => x.Edrpou.Length > 0)) result.byCode.TryAdd(x.Edrpou, Of(x));
        if (codes.Count > 0)
            foreach (var client in await customers.FindByTaxIdsAsync(codes, cancellationToken))
                result.byCode.TryAdd(client.TaxId.Trim(), External(CounterpartyKind.Client, client));
        if (codes.Count > 0)
            foreach (var supplier in (await suppliers.ListAsync(cancellationToken)).Where(x => x.Active && x.TaxId.Length > 0))
                result.byCode.TryAdd(supplier.TaxId.Trim(), External(CounterpartyKind.Supplier, supplier));

        foreach (var x in financial.Where(x => x.Iban.Length > 0)) result.byIban.TryAdd(x.Iban, Of(x));
        foreach (var x in financial)
            foreach (var name in new[] { x.Name, x.BankName }.Concat(x.Details.AliasList))
                if (Key(name) is { Length: > 0 } key) result.byName.TryAdd(key, Of(x));
        return result;
    }

    /// <summary>By ЄДРПОУ, then IBAN, then the bank name (or, in a card statement without one, the description).</summary>
    public BankParty? Find(BankStatementLine line) =>
        (line.Edrpou.Length > 0 ? byCode.GetValueOrDefault(line.Edrpou) : null)
        ?? (line.Iban.Length > 0 ? byIban.GetValueOrDefault(line.Iban) : null)
        ?? byName.GetValueOrDefault(Key(line.BankName.Length > 0 ? line.BankName : line.Purpose));

    private static BankParty External(CounterpartyKind kind, AccountingParty party) =>
        new(new CounterpartyState(CounterpartyLink.Of(kind, party.Id), party.Name, party.Active, Verified: true), null);

    private static string Key(string text) => FinanceText.Name(text).ToLowerInvariant();
}

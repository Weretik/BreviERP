using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Application.Features.Counterparties.Shared;

namespace Accounting.Application.Features.Lookup.Shared;

/// <summary>
/// Account and category names for journal rows; both lists are small, so they are read whole. Counterparty names are
/// added for the rows at hand with <see cref="With"/>.
/// </summary>
internal sealed class FinanceNames(IEnumerable<FinanceAccount> accounts, IEnumerable<FinanceCategory> categories)
{
    private readonly Dictionary<FinanceAccountId, FinanceAccount> accounts = accounts.ToDictionary(x => x.Id);
    private readonly Dictionary<FinanceCategoryId, FinanceCategory> categories = categories.ToDictionary(x => x.Id);

    public CounterpartyStates Counterparties { get; private set; } = CounterpartyStates.Empty;

    public FinanceNames With(CounterpartyStates counterparties)
    {
        Counterparties = counterparties;
        return this;
    }

    public static async Task<FinanceNames> LoadAsync(IAccountingDbContext db, CancellationToken cancellationToken) =>
        new(await db.Accounts.AsNoTracking().ToListAsync(cancellationToken),
            await db.Categories.AsNoTracking().ToListAsync(cancellationToken));

    public string Account(FinanceAccountId id) => accounts.TryGetValue(id, out var account) ? account.Name : string.Empty;

    public FinanceCategory? Category(FinanceCategoryId? id) =>
        id is { } value && categories.TryGetValue(value, out var category) ? category : null;

    /// <summary>The root of a category: the category itself for a root.</summary>
    public FinanceCategory? Root(FinanceCategory? category) =>
        category?.ParentId is { } parent ? Category(parent) : category;

    /// <summary>Ids of a category and, for a root, of its subcategories.</summary>
    public IReadOnlyList<FinanceCategoryId?> Branch(FinanceCategoryId id) =>
        categories.Values.Where(x => x.Id == id || x.ParentId == id).Select(x => (FinanceCategoryId?)x.Id).ToList();
}

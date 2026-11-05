using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;

namespace Accounting.Application.Categories;

internal static class CategoryNames
{
    /// <summary>
    /// True when another category of the same area, direction and level (same root, or both roots) already has this
    /// name, ignoring case.
    /// </summary>
    public static async Task<bool> TakenAsync(IAccountingDbContext db, string name, CategoryDirection direction,
        LedgerScope scope, FinanceCategoryId? parentId, FinanceCategoryId? except, CancellationToken cancellationToken)
    {
        var clean = FinanceText.Name(name);
        var siblings = await db.Categories.AsNoTracking()
            .Where(x => x.Direction == direction && x.Scope == scope && x.ParentId == parentId)
            .Select(x => new { x.Id, x.Name })
            .ToListAsync(cancellationToken);
        return siblings.Any(x => x.Id != except && string.Equals(x.Name, clean, StringComparison.CurrentCultureIgnoreCase));
    }
}

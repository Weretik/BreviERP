using Accounting.Domain.Counterparties;
using Accounting.Application.Features.Lookup.Shared;

namespace Accounting.Application.Features.Counterparties.Shared;

/// <summary>
/// A finance counterparty of «Контрагенти → Фінансові». <c>DefaultCategoryId</c> is the chosen category (a
/// subcategory or a root); <c>DefaultCategoryRootName</c>/<c>DefaultSubcategoryName</c> name it.
/// <c>OperationCount</c> counts active operations of both areas; <c>CurrentTurnoverMinor</c> is incomes minus expenses
/// of the current books, in kopecks.
/// </summary>
public sealed record FinanceCounterpartyResponse(
    Guid Id,
    string Name,
    string Edrpou,
    string Iban,
    string BankName,
    IReadOnlyList<string> Aliases,
    Guid? DefaultCategoryId,
    string? DefaultCategoryRootName,
    string? DefaultSubcategoryName,
    bool AutoApplyCategory,
    string Note,
    bool Verified,
    bool Active,
    Guid? MergedIntoId,
    int OperationCount,
    long CurrentTurnoverMinor,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    internal static FinanceCounterpartyResponse From(FinanceCounterparty x, FinanceNames names,
        FinanceOperationStats stats = default)
    {
        var category = names.Category(x.DefaultCategoryId);
        var root = names.Root(category);
        return new FinanceCounterpartyResponse(x.Id.Value, x.Name, x.Edrpou, x.Iban, x.BankName, x.Details.AliasList,
            x.DefaultCategoryId?.Value, root?.Name, category is { IsRoot: false } ? category.Name : null,
            x.AutoApplyCategory, x.Note, x.Verified, x.Active, x.MergedIntoId?.Value, stats.Count, stats.CurrentTurnoverMinor,
            x.Version,
            x.CreatedAt, x.UpdatedAt);
    }
}

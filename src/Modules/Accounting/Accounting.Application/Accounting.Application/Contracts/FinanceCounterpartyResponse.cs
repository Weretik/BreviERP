using Accounting.Application.Counterparties;
using Accounting.Application.Lookup;
using Accounting.Domain.Counterparties;

namespace Accounting.Application.Contracts;

/// <summary>
/// Who an operation is with. <c>Id</c> is text: a UUID for a finance counterparty and a client, a number for a
/// supplier. <c>Missing</c> is true when the record is gone from its list (the link stays).
/// </summary>
public sealed record FinanceCounterpartyRefResponse(CounterpartyKind Kind, string Id, string Name, bool Active,
    bool Verified, bool Missing)
{
    internal static FinanceCounterpartyRefResponse? From(CounterpartyLink? link, FinanceNames names)
    {
        if (link is null) return null;
        var state = names.Counterparties.Of(link);
        return state is null
            ? new FinanceCounterpartyRefResponse(link.Kind, link.Id, string.Empty, false, true, true)
            : new FinanceCounterpartyRefResponse(link.Kind, link.Id, state.Name, state.Active, state.Verified, false);
    }
}

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

/// <summary>
/// A client of «Клієнти» or a supplier of «Постачальники» in «Контрагенти»; read-only here. <c>CurrentTurnoverMinor</c>
/// is incomes minus expenses of the current books.
/// </summary>
public sealed record FinancePartyResponse(CounterpartyKind Kind, string Id, string Name, string TaxId, bool Active,
    int OperationCount, long CurrentTurnoverMinor);

/// <summary>
/// One page of «Контрагенти» for one tab: <c>Financial</c> for «Фінансові», <c>Parties</c> for «Клієнти» and
/// «Постачальники», <c>Oneoffs</c> — operations with a one-off name — for «Разові». <c>Counts</c> are per tab with the
/// same search. 50 rows per page.
/// </summary>
public sealed record FinanceCounterpartyPageResponse(
    FinanceCounterpartyTab Tab,
    IReadOnlyList<FinanceCounterpartyResponse> Financial,
    IReadOnlyList<FinancePartyResponse> Parties,
    IReadOnlyList<FinanceTransactionResponse> Oneoffs,
    int Total,
    int Page,
    int PageSize,
    FinanceCounterpartyCountsResponse Counts);

public sealed record FinanceCounterpartyCountsResponse(int Financial, int Clients, int Suppliers, int Oneoffs);

/// <summary>A choice of the counterparty field of an operation; <c>Hint</c> is ЄДРПОУ/ІПН or empty.</summary>
public sealed record FinanceCounterpartyChoiceResponse(CounterpartyKind Kind, string Id, string Name, string Hint,
    bool Verified, Guid? DefaultCategoryId);

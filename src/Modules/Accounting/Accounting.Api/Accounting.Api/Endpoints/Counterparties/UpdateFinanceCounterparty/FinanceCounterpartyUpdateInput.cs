using Accounting.Application.Features.Counterparties.Shared;

namespace Accounting.Api.Endpoints.Counterparties.UpdateFinanceCounterparty;

/// <summary>Changed finance counterparty with the <c>Version</c> the user opened.</summary>
public sealed record FinanceCounterpartyUpdateInput(
    string? Name,
    string? Edrpou,
    string? Iban,
    string? BankName,
    IReadOnlyList<string>? Aliases,
    Guid? DefaultCategoryId,
    bool AutoApplyCategory,
    string? Note,
    int Version)
{
    public FinanceCounterpartyFields ToFields() =>
        new(Name, Edrpou, Iban, BankName, Aliases, DefaultCategoryId, AutoApplyCategory, Note);
}

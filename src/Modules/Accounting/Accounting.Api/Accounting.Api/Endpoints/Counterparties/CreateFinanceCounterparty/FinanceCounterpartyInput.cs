using Accounting.Application.Features.Counterparties.Shared;

namespace Accounting.Api.Endpoints.Counterparties.CreateFinanceCounterparty;

/// <summary>
/// Finance counterparty form. <c>Aliases</c> are other spellings, <c>DefaultCategoryId</c> a category of the current
/// books; <c>Verified</c> on create is true only for «Зберегти як фінансового контрагента» from an operation.
/// </summary>
public sealed record FinanceCounterpartyInput(
    string? Name,
    string? Edrpou,
    string? Iban,
    string? BankName,
    IReadOnlyList<string>? Aliases,
    Guid? DefaultCategoryId,
    bool AutoApplyCategory,
    string? Note,
    bool Verified)
{
    public FinanceCounterpartyFields ToFields() =>
        new(Name, Edrpou, Iban, BankName, Aliases, DefaultCategoryId, AutoApplyCategory, Note);
}

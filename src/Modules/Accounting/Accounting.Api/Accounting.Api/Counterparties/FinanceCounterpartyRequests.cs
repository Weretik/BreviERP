using Accounting.Application.Features.Counterparties.Shared;

namespace Accounting.Api.Counterparties;

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

/// <summary>«Об'єднати»: the target and both versions the user saw.</summary>
public sealed record FinanceCounterpartyMergeInput(Guid TargetId, int Version, int TargetVersion);

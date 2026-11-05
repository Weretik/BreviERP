namespace Crm.Application.Embroidery.Contracts;

/// <summary>
/// One calculation of the old calculator's archive, as the transfer file holds it: the form, the rates it was saved
/// with and the prices the old calculator saved.
/// </summary>
/// <param name="LegacyId">The old record id; a record already moved is left as it is.</param>
/// <param name="CreatedByEmail">The old author; matched to a staff member by email, otherwise shown as the email.</param>
/// <param name="StitchRate">The embroiderer's rate per 1000 stitches at save time.</param>
/// <param name="Settings">The old «Налаштування» at save time; <c>Version</c> is ignored.</param>
/// <param name="Prices">The old saved prices, «Опт», «Стандарт», «Терміново» in order.</param>
public sealed record EmbroideryArchiveRow(
    string LegacyId,
    string? Name,
    DateTimeOffset CreatedAt,
    string? CreatedByEmail,
    CalculateEmbroideryInput Calculation,
    decimal StitchRate,
    SaveEmbroiderySettingsInput Settings,
    IReadOnlyList<EmbroideryArchivePrice> Prices);

public sealed record EmbroideryArchivePrice(decimal Unit, decimal Total, decimal RepeatUnit);

/// <param name="DryRun">True when nothing was saved: a check, or a list with errors.</param>
public sealed record EmbroideryArchiveImportResult(
    bool DryRun,
    int Created,
    int Unchanged,
    int Errors,
    IReadOnlyList<EmbroideryArchiveImportLine> Lines);

/// <param name="Action">`created`, `unchanged` or `error`.</param>
/// <param name="Number">«ВШ-0042» once saved; null in a check.</param>
public sealed record EmbroideryArchiveImportLine(
    string LegacyId,
    string Name,
    DateTimeOffset CreatedAt,
    string CreatedByName,
    string Action,
    string? Message,
    string? Number);

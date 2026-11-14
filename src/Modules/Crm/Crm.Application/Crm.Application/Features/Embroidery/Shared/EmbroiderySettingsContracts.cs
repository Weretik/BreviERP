namespace Crm.Application.Features.Embroidery.Shared;

/// <param name="TaxPercent">«Податкове навантаження»: a multiplier after the markup, not VAT.</param>
public sealed record EmbroideryRatesDto(decimal ThreadRate, decimal HoopingRate, decimal ProgramHourRate, decimal TaxPercent);

public sealed record EmbroideryMarkupDto(string Name, decimal Percent);

public sealed record EmbroideryMaterialDto(decimal PricePerMetre, decimal WidthCm);

/// <param name="GlueRate">Null: the general «Клей» rate is used.</param>
public sealed record EmbroideryHoopDto(string Id, string Label, decimal BlankWidthCm, decimal BlankLengthCm, decimal? GlueRate);

/// <param name="Basis">`Stitches`, `Hoop`, `Unit`, `MachineHour` or `Order`.</param>
/// <param name="Rate">Null: not set yet (the price becomes «Орієнтовна»); 0 is free.</param>
/// <param name="Mode">`All` or `ChevronOnly`.</param>
public sealed record EmbroideryExpenseDto(string Id, string Name, string Basis, decimal? Rate, bool Enabled, string Mode);

/// <summary>The whole «Налаштування» page; <paramref name="Version"/> is the one read (0 before the first save).</summary>
public sealed record SaveEmbroiderySettingsInput(
    int Version,
    EmbroideryRatesDto Rates,
    IReadOnlyList<EmbroideryMarkupDto> Markups,
    EmbroideryMaterialDto Stabilizer,
    EmbroideryMaterialDto Fabric,
    decimal FabricIncreasePercent,
    IReadOnlyList<EmbroideryHoopDto> Hoops,
    IReadOnlyList<EmbroideryExpenseDto> Expenses);

/// <summary>The embroiderer's rate, read-only here: it is edited in «Параметри ціноутворення».</summary>
public sealed record EmbroideryStitchRateResponse(decimal? Value, DateOnly? UpdatedOn);

/// <summary>«Налаштування»: the saved record, or the old calculator's rates before the first save (Version 0).</summary>
public sealed record EmbroiderySettingsResponse(
    int Version,
    string? UpdatedByName,
    DateTimeOffset? UpdatedAt,
    EmbroideryStitchRateResponse StitchRate,
    EmbroideryRatesDto Rates,
    IReadOnlyList<EmbroideryMarkupDto> Markups,
    EmbroideryMaterialDto Stabilizer,
    EmbroideryMaterialDto Fabric,
    decimal FabricIncreasePercent,
    IReadOnlyList<EmbroideryHoopDto> Hoops,
    IReadOnlyList<EmbroideryExpenseDto> Expenses);

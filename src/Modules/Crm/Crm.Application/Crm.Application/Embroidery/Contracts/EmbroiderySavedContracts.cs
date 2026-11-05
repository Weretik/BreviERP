namespace Crm.Application.Embroidery.Contracts;

/// <summary>«Зберегти розрахунок»: the server prices the form again with today's rates and freezes them.</summary>
/// <param name="Name">Optional; trimmed and cut to 120 characters.</param>
/// <param name="CustomerPublicId">A Crm client the calculation is for, optional.</param>
public sealed record SaveEmbroideryCalculationInput(
    string? Name,
    Guid? CustomerPublicId,
    CalculateEmbroideryInput Calculation);

/// <summary>A price of a saved calculation as the list shows it; no markup percents.</summary>
public sealed record EmbroiderySavedPriceResponse(string Name, decimal Unit, decimal Total, decimal RepeatUnit);

/// <summary>One row of «Збережені».</summary>
/// <param name="Number">«ВШ-0042».</param>
/// <param name="Mode">`Garment` or `Chevron`.</param>
/// <param name="HoopLabel">The hoop label at save time.</param>
/// <param name="Incomplete">Some ticked costs had no rate: the prices were «Орієнтовна».</param>
/// <param name="Mockup">«Макет» from the Media library; null when none or the photo was removed.</param>
public sealed record EmbroiderySavedSummaryResponse(
    long Id,
    string Number,
    string Name,
    Guid? CustomerPublicId,
    string? CustomerName,
    string Mode,
    int Quantity,
    int StitchesPerUnit,
    string HoopLabel,
    bool Incomplete,
    IReadOnlyList<EmbroiderySavedPriceResponse> Prices,
    DateTimeOffset CreatedAt,
    string CreatedByName,
    EmbroideryMockupResponse? Mockup);

/// <summary>A photo or mockup of the design for the workshop.</summary>
public sealed record EmbroideryMockupResponse(int MediaFileId, string FileName, string Url, string? ThumbnailUrl);

/// <summary>«Макет»: a Media library photo, or null to clear it.</summary>
public sealed record SetEmbroideryMockupInput(int? MediaFileId);

public sealed record EmbroiderySavedPageResponse(
    IReadOnlyList<EmbroiderySavedSummaryResponse> Items,
    int Page,
    int PageSize,
    int Total);

/// <summary>The rates a calculation was saved with; Director and Admin only.</summary>
public sealed record EmbroiderySnapshotRatesResponse(
    decimal StitchRate,
    EmbroideryRatesDto Rates,
    IReadOnlyList<EmbroideryMarkupDto> Markups);

/// <summary>
/// A saved calculation: the form as it was (to repeat it with today's rates), the saved result (a Manager gets no
/// cost) and, for Director and Admin, the frozen rates.
/// </summary>
public sealed record EmbroiderySavedCalculationResponse(
    EmbroiderySavedSummaryResponse Summary,
    CalculateEmbroideryInput Input,
    EmbroideryCalculationResponse Result,
    EmbroiderySnapshotRatesResponse? Rates);

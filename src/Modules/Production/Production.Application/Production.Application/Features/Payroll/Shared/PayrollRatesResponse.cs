namespace Production.Application.Features.Payroll.Shared;

/// <summary>Shares from «Параметри ціноутворення» as fractions (0.08 = 8 %); null when not filled in.</summary>
public sealed record PayrollRatesResponse(decimal? Bonus, decimal? BonusThreshold, decimal? Cutting, decimal? Master,
    decimal? Chief, decimal? Embroidery);

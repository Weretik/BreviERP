namespace Crm.Application.Abstractions.Embroidery;

/// <summary>
/// «Оплата фактичної вишивки за 1000 стібків» from «Параметри ціноутворення» (Reference): the embroiderer is paid and
/// the price is built from the same number.
/// </summary>
public interface IEmbroideryStitchRateReader
{
    /// <summary>Null value when the parameter is missing or empty.</summary>
    Task<EmbroideryStitchRate> ReadAsync(CancellationToken cancellationToken);
}

public sealed record EmbroideryStitchRate(decimal? Value, DateOnly? UpdatedOn);

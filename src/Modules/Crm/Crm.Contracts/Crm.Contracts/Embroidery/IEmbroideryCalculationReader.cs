namespace Crm.Contracts.Embroidery;

/// <summary>A saved «Ціна вишивки» calculation without costs and prices, for the workshop.</summary>
public sealed record EmbroideryCalculationInfo(
    long Id, string Number, string Name, string? CustomerName, string Mode, int Quantity, int StitchesPerUnit,
    string HoopLabel, int Hoopings, decimal? StabilizerMetres, decimal? FabricMetres, string? MockupUrl,
    string? MockupThumbnailUrl);

/// <summary>A calculation in the picker list.</summary>
public sealed record EmbroideryChoiceInfo(long Id, string Number, string Name, string? CustomerName, int Quantity,
    int StitchesPerUnit, string HoopLabel, string? MockupThumbnailUrl);

/// <summary>Saved embroidery calculations of Crm for other modules; <paramref name="userId"/> reads as a non-admin.</summary>
public interface IEmbroideryCalculationReader
{
    Task<IReadOnlyList<EmbroideryChoiceInfo>> SearchAsync(int userId, string? search, int max,
        CancellationToken cancellationToken);

    /// <summary>The calculations of <paramref name="ids"/> in one read; an id that is not found is left out.</summary>
    Task<IReadOnlyList<EmbroideryCalculationInfo>> FindManyAsync(int userId, IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken);
}

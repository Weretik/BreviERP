namespace Crm.Contracts.Embroidery;

/// <summary>A saved «Ціна вишивки» calculation without costs and prices, for the workshop.</summary>
public sealed record EmbroideryCalculationInfo(
    long Id, string Number, string Name, string? CustomerName, string Mode, int Quantity, int StitchesPerUnit,
    string HoopLabel, int Hoopings, decimal? StabilizerMetres, decimal? FabricMetres, string? MockupUrl,
    string? MockupThumbnailUrl);

/// <summary>A calculation in the picker list.</summary>
public sealed record EmbroideryChoiceInfo(long Id, string Number, string Name, string? CustomerName, int Quantity,
    int StitchesPerUnit, string HoopLabel, string? MockupThumbnailUrl);

/// <summary>A library photo used as the «Макет» of a calculation.</summary>
public sealed record EmbroideryMockupUsage(int MediaFileId, long CalculationId, string Title);

/// <summary>Saved embroidery calculations of Crm for other modules; <paramref name="userId"/> reads as a non-admin.</summary>
public interface IEmbroideryCalculationReader
{
    Task<IReadOnlyList<EmbroideryChoiceInfo>> SearchAsync(int userId, string? search, int max,
        CancellationToken cancellationToken);

    Task<EmbroideryCalculationInfo?> FindAsync(int userId, long id, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmbroideryMockupUsage>> MockupUsagesAsync(IReadOnlyCollection<int> mediaFileIds,
        CancellationToken cancellationToken);
}
